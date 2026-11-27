import json,sys,os,re,collections,glob,datetime
root=sys.argv[1]
files=glob.glob(root+'/**/*.jsonl',recursive=True)
pipe_re=re.compile(r'(?<!\|)\|(?!\|)')
EXP=re.compile(r'(\bdev (build|test|audit)\b|dotnet (build|test|run|publish)|ganda repo audit|ganda (task|pr|repo)\b.*\b(audit|attest|verify)|run-tests|\bnpm (test|run)|cargo (build|test)|bin/dev )')
seen=set()
st=collections.Counter(); forms=collections.Counter(); nsl=collections.Counter()
rer=[]; tmin=None; tmax=None
def ts(s):
    try: return datetime.datetime.fromisoformat(s.replace('Z','+00:00'))
    except: return None
def parse_slice(seg):
    seg=re.split(r'&&|;|\n|\)|`|2>&1|>',seg)[0].strip()
    t=seg.split()
    if not t or os.path.basename(t[0]) not in('head','tail'): return None
    a=t[1:]
    if not a: return (t[0],'none',10)
    j=' '.join(a)
    m=re.fullmatch(r'-(\d+)',j) or re.fullmatch(r'-n ?(\d+)',j) or re.fullmatch(r'--lines[= ](\d+)',j)
    if m: return (t[0],'-N' if j.startswith('-') and not j.startswith('-n') and not j.startswith('--') else '-n N',int(m.group(1)))
    if re.search(r'-n ?\+|\s\+\d|^\+\d',j): return (t[0],'+N',None)
    if re.search(r'(^|\s)-c|--bytes',j): return (t[0],'-c',None)
    if re.search(r'(^|\s)-[fF]\b|--follow',j): return (t[0],'-f',None)
    if re.search(r'-q|-v|-z',j): return (t[0],'other-flag',None)
    return (t[0],'file-arg',None)
for f in files:
    uses=[]; res={}; tuse={}
    sess=os.path.basename(f)
    for l in open(f,errors='replace'):
        if '"Bash"' not in l and 'tool_result' not in l: continue
        try: d=json.loads(l)
        except: continue
        m=d.get('message')
        if not isinstance(m,dict) or not isinstance(m.get('content'),list): continue
        t=ts(d.get('timestamp',''))
        for b in m['content']:
            if not isinstance(b,dict): continue
            if b.get('type')=='tool_use' and b.get('name')=='Bash':
                if b['id'] in seen: continue
                seen.add(b['id'])
                c=b.get('input',{}).get('command','')
                if isinstance(c,str): uses.append((b['id'],c,t))
                if t:
                    tmin=t if not tmin or t<tmin else tmin; tmax=t if not tmax or t>tmax else tmax
            elif b.get('type')=='tool_result':
                cc=b.get('content')
                if isinstance(cc,list): cc='\n'.join(x.get('text','') for x in cc if isinstance(x,dict))
                res[b.get('tool_use_id')]=(str(cc),t)
    for i,(uid,cmd,t) in enumerate(uses):
        st['bash']+=1
        # skip heredoc python bodies: cut at first <<
        body=cmd.split('<<')[0]
        segs=pipe_re.split(body)
        hit=False
        for j,s in enumerate(segs):
            for k,sub in enumerate(re.split(r'&&|;|\n|\$\(',s)):
                p=parse_slice(sub)
                if not p: continue
                piped = (j>0 and k==0)
                forms[('pipe ' if piped else 'nopipe ')+p[0]+' '+p[1]]+=1
                hit=True
                if not piped or p[2] is None: continue
                up=' '.join('|'.join(segs[:j]).split())
                up=re.sub(r'^.*(&&|;)\s*','',up).strip()  # last statement feeding pipe
                exp=bool(EXP.search(up))
                out,rt=res.get(uid,('',None))
                dur=(rt-t).total_seconds() if rt and t else None
                cut = out.count('\n')+1 >= p[2]
                key='exp' if exp else 'cheap'
                nsl[key+' slices']+=1
                if cut: nsl[key+' cut-likely']+=1
                if len(up)<8: continue
                for uid2,c2,t2 in uses[i+1:i+6]:
                    n2=' '.join(c2.split())
                    if up in n2 and n2!=' '.join(cmd.split()):
                        o2,rt2=res.get(uid2,('',None))
                        d2=(rt2-t2).total_seconds() if rt2 and t2 else None
                        nsl[key+' rerun']+=1
                        if cut: nsl[key+' rerun-after-cut']+=1
                        rer.append(dict(sess=sess,proj=os.path.basename(os.path.dirname(f))[-50:],up=up[:150],slice=p[0]+' '+str(p[2]),cut=cut,dur1=dur,dur2=d2,next=n2[:220],ts=str(t)))
                        break
        if hit: st['bash with head/tail']+=1
print(st, 'window',tmin,tmax)
for k,v in forms.most_common(30): print(v,k)
print(dict(nsl))
json.dump(rer,open('reruns.json','w'),indent=0)
