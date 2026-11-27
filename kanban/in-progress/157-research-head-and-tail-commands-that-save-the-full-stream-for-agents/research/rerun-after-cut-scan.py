# Task 157: count re-runs of the same upstream after a head/tail cut (Q8, Q9).
# Usage: python3 -I rerun-after-cut-scan.py <claude projects folder> [--until YYYY-MM-DD] [--rows FILE]
# A cut is a piped `-N`, `-n N`, or no-argument slice whose tool result has at least N lines.
# Loose re-run: the upstream text appears in one of the next 4 Bash calls, with no edit tool
#   and no mutating bash (MUT) in between.
# Strict re-run: a loose re-run whose upstream is expensive (STRICT) and where one statement
#   of the next command (split on &&, ;, newline) starts with that exact upstream.
import json,sys,os,re,collections,glob,datetime
args=sys.argv[1:]
until=None; rows_path=None
if '--until' in args:
    i=args.index('--until'); until=datetime.datetime.fromisoformat(args[i+1]+'T00:00:00+00:00'); del args[i:i+2]
if '--rows' in args:
    i=args.index('--rows'); rows_path=args[i+1]; del args[i:i+2]
root=args[0]
files=glob.glob(root+'/**/*.jsonl',recursive=True)
pipe_re=re.compile(r'(?<!\|)\|(?!\|)')
EXP=re.compile(r'(\bdev (build|test|audit)\b|dotnet (build|test|run|publish)|ganda repo audit|run-tests|bin/dev )')
AUD=re.compile(r'ganda repo audit|dev audit')
MUT=re.compile(r'sed -i|> *[^&/ ]+\.(cs|md|json|props|csproj)|git (commit|merge|pull|checkout|switch|stash|rebase)|dotnet format|--fix|cat > |tee ')
# Strict rows, first match wins. The label is the Q8 table row.
STRICT=[
    ('ganda repo audit',re.compile(r'ganda repo audit|\bdev audit\b')),
    ('dev test',re.compile(r'\bdev test\b')),
    ('dev build',re.compile(r'\bdev build\b')),
    ('dev deps',re.compile(r'\bdev deps\b')),
    ('dotnet test/run',re.compile(r'dotnet (test|run)\b')),
    ('dotnet build/publish',re.compile(r'dotnet (build|publish)\b')),
    ('other bin/dev or run-tests',re.compile(r'bin/dev |run-tests')),
]
seen=set(); out=collections.Counter(); rows=[]
strict=collections.Counter(); strict_sec=collections.Counter()
def ts(s):
    try: return datetime.datetime.fromisoformat(s.replace('Z','+00:00'))
    except Exception: return None
def nlines(o):
    # A trailing newline ends the last line; it does not start a new one.
    return len(o.splitlines())
def sl(seg):
    seg=re.split(r'&&|;|\n|\)|`|2>&1|>',seg)[0].strip(); t=seg.split()
    if not t or os.path.basename(t[0]) not in('head','tail'): return None
    tool=os.path.basename(t[0])
    a=' '.join(t[1:])
    if not a: return (tool,10)
    m=re.fullmatch(r'-(\d+)',a) or re.fullmatch(r'-n ?(\d+)',a)
    return (tool,int(m.group(1))) if m else None
def strict_row(up):
    for label,rx in STRICT:
        if rx.search(up): return label
    return None
def starts_a_statement(nc,up):
    return any(s.strip().startswith(up) for s in re.split(r'&&|;|\n',nc))
for f in files:
    ev=[]; res={}
    for l in open(f,errors='replace'):
        if 'tool_use' not in l and 'tool_result' not in l: continue
        try: d=json.loads(l)
        except Exception: continue
        m=d.get('message')
        if not isinstance(m,dict) or not isinstance(m.get('content'),list): continue
        t=ts(d.get('timestamp',''))
        for b in m['content']:
            if not isinstance(b,dict): continue
            if b.get('type')=='tool_use':
                if b.get('id') in seen: continue
                seen.add(b.get('id'))
                if until and t and t>=until: continue
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
            exp=bool(EXP.search(up)); aud=bool(AUD.search(up)); srow=strict_row(up)
            o,rt=res.get(uid,('',None)); dur=(rt-t).total_seconds() if rt and t else None
            cut=nlines(o)>=p[1]
            k='exp' if exp else 'cheap'
            out[k+' slice']+=1
            if not cut: break
            out[k+' cut']+=1
            if srow: out['strict-upstream cut']+=1
            steps=0; rerun=False
            for uid2,n2,c2,t2 in ev[i+1:i+12]:
                if n2 in('Edit','Write','MultiEdit','NotebookEdit') or (n2=='Bash' and MUT.search(c2)): break
                if n2!='Bash': continue
                steps+=1
                if steps>4: break
                nc=' '.join(c2.split())
                if up in nc and nc!=' '.join(cmd.split()):
                    rerun=True
                    o2,rt2=res.get(uid2,('',None)); d2=(rt2-t2).total_seconds() if rt2 and t2 else None
                    out['loose rerun']+=1
                    out[k+' loose rerun']+=1
                    if d2 and d2>=30: out['loose rerun >=30s']+=1; out['loose rerun >=30s seconds']+=d2
                    is_strict=bool(srow) and starts_a_statement(nc,up)
                    if is_strict:
                        strict['total']+=1
                        strict['after '+p[0]]+=1
                        strict['month '+str(t)[:7]]+=1
                        strict['row '+srow]+=1
                        strict_sec['total']+=dur or 0
                        strict_sec['row '+srow]+=dur or 0
                    rows.append(dict(proj=os.path.basename(os.path.dirname(f))[-55:],up=up[:140],slice=f'{p[0]} {p[1]}',strict=is_strict,dur1=dur,dur2=d2,next=nc[:200],ts=str(t)[:10]))
                    break
            if srow and not rerun: out['strict-upstream cut, not rerun']+=1
            break
print('-- loose')
for k in sorted(out): print(round(out[k]),k)
print('-- strict (cases; seconds = first run duration)')
for k in sorted(strict): print(strict[k],k,round(strict_sec[k]) if k in strict_sec or k=='total' else '')
if rows_path: json.dump(rows,open(rows_path,'w'),indent=0)
