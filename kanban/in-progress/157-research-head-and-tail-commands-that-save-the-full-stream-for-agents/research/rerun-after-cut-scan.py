import json,sys,os,re,collections,glob,datetime
root=sys.argv[1]
files=glob.glob(root+'/**/*.jsonl',recursive=True)
pipe_re=re.compile(r'(?<!\|)\|(?!\|)')
EXP=re.compile(r'(\bdev (build|test|audit)\b|dotnet (build|test|run|publish)|ganda repo audit|run-tests|bin/dev )')
AUD=re.compile(r'ganda repo audit|dev audit')
MUT=re.compile(r'sed -i|> *[^&/ ]+\.(cs|md|json|props|csproj)|git (commit|merge|pull|checkout|switch|stash|rebase)|dotnet format|--fix|cat > |tee ')
seen=set(); out=collections.Counter(); rows=[]
def ts(s):
    try: return datetime.datetime.fromisoformat(s.replace('Z','+00:00'))
    except: return None
def sl(seg):
    seg=re.split(r'&&|;|\n|\)|`|2>&1|>',seg)[0].strip(); t=seg.split()
    if not t or t[0] not in('head','tail'): return None
    a=' '.join(t[1:])
    if not a: return (t[0],10)
    m=re.fullmatch(r'-(\d+)',a) or re.fullmatch(r'-n ?(\d+)',a)
    return (t[0],int(m.group(1))) if m else None
for f in files:
    ev=[]; res={}
    for l in open(f,errors='replace'):
        if 'tool_use' not in l and 'tool_result' not in l: continue
        try: d=json.loads(l)
        except: continue
        m=d.get('message')
        if not isinstance(m,dict) or not isinstance(m.get('content'),list): continue
        t=ts(d.get('timestamp',''))
        for b in m['content']:
            if not isinstance(b,dict): continue
            if b.get('type')=='tool_use':
                if b.get('id') in seen: continue
                seen.add(b.get('id'))
                c=b.get('input',{}).get('command','') if b.get('name')=='Bash' else ''
                ev.append((b.get('id'),b.get('name'),c if isinstance(c,str) else '',t))
            elif b.get('type')=='tool_result':
                cc=b.get('content')
                if isinstance(cc,list): cc='\n'.join(x.get('text','') for x in cc if isinstance(x,dict))
                res[b.get('tool_use_id')]=(str(cc),t)
    for i,(uid,name,cmd,t) in enumerate(ev):
        if name!='Bash' or not cmd: continue
        body=cmd.split('<<')[0]; segs=pipe_re.split(body)
        for j in range(1,len(segs)):
            p=sl(segs[j])
            if not p: continue
            up=' '.join('|'.join(segs[:j]).split())
            up=re.sub(r'^.*(&&|;)\s*','',up).strip()
            if len(up)<8: break
            exp=bool(EXP.search(up)); aud=bool(AUD.search(up))
            o,rt=res.get(uid,('',None)); dur=(rt-t).total_seconds() if rt and t else None
            cut=o.count('\n')+1>=p[1]
            k='exp' if exp else 'cheap'
            out[k+' slice']+=1
            if aud: out['audit slice']+=1
            if dur is not None and dur>=30: out['>=30s slice']+=1
            if not cut: break
            out[k+' cut']+=1
            if dur is not None and dur>=30: out['>=30s cut']+=1
            steps=0
            for uid2,n2,c2,t2 in ev[i+1:i+12]:
                if n2 in('Edit','Write','MultiEdit','NotebookEdit') or (n2=='Bash' and MUT.search(c2)): break
                if n2!='Bash': continue
                steps+=1
                if steps>4: break
                nc=' '.join(c2.split())
                if up in nc and nc!=' '.join(cmd.split()):
                    o2,rt2=res.get(uid2,('',None)); d2=(rt2-t2).total_seconds() if rt2 and t2 else None
                    out[k+' rerun-no-edit']+=1
                    if aud: out['audit rerun-no-edit']+=1
                    if d2: out[k+' rerun seconds']+=d2
                    if d2 and d2>=30: out['>=30s rerun']+=1; out['>=30s rerun seconds']+=d2
                    rows.append(dict(proj=os.path.basename(os.path.dirname(f))[-55:],up=up[:140],slice=f'{p[0]} {p[1]}',dur1=dur,dur2=d2,next=nc[:200],ts=str(t)[:10]))
                    break
            break
for k in sorted(out): print(k,round(out[k]))
json.dump(rows,open('rows.json','w'),indent=0)
