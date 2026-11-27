# Task 157: count head/tail forms in Claude transcripts (Q1, Q3, Q5).
# Usage: python3 -I slice-forms-scan.py <claude projects folder> [--until YYYY-MM-DD]
# Bash calls are deduped by tool-use id. --until drops calls at or after that UTC date.
import json,sys,os,re,collections,glob,datetime
args=sys.argv[1:]
until=None
if '--until' in args:
    i=args.index('--until'); until=datetime.datetime.fromisoformat(args[i+1]+'T00:00:00+00:00'); del args[i:i+2]
root=args[0]
files=glob.glob(root+'/**/*.jsonl',recursive=True)
pipe_re=re.compile(r'(?<!\|)\|(?!\|)')
SLICE=r'\|\s*(?:\S*/)?(?:head|tail)\b'
CAPTURE_RE=re.compile(r'\$\([^)]*'+SLICE)
CAPTURE_MERGE_RE=re.compile(r'\$\([^)]*'+SLICE+r'[^)]*2>&1[^)]*\)')
GROUP_MERGE_RE=re.compile(r'(?:\{[^}]*'+SLICE+r'[^}]*\}|(?<!\$)\([^()]*'+SLICE+r'[^()]*\))\s*2>&1')
seen=set()
st=collections.Counter(); forms=collections.Counter(); nsl=collections.Counter()
tmin=None; tmax=None
def ts(s):
    try: return datetime.datetime.fromisoformat(s.replace('Z','+00:00'))
    except Exception: return None
def nlines(out):
    # A trailing newline ends the last line; it does not start a new one.
    return len(out.splitlines())
def parse_slice(seg):
    seg=re.split(r'&&|;|\n|\)|`|2>&1|>',seg)[0].strip()
    t=seg.split()
    if not t or os.path.basename(t[0]) not in('head','tail'): return None
    tool=os.path.basename(t[0])
    a=t[1:]
    if not a: return (tool,'none',10)
    j=' '.join(a)
    m=re.fullmatch(r'-(\d+)',j) or re.fullmatch(r'-n ?(\d+)',j) or re.fullmatch(r'--lines[= ](\d+)',j)
    if m: return (tool,'-N' if j.startswith('-') and not j.startswith('-n') and not j.startswith('--') else '-n N',int(m.group(1)))
    if re.search(r'-n ?\+|\s\+\d|^\+\d',j): return (tool,'+N',None)
    if re.search(r'(^|\s)-c|--bytes',j): return (tool,'-c',None)
    if re.search(r'(^|\s)-[fF]\b|--follow',j): return (tool,'-f',None)
    if re.search(r'-q|-v|-z',j): return (tool,'other-flag',None)
    return (tool,'file-arg',None)
def slice_merges_stderr(seg):
    # `| tail -1 2>&1`: the 2>&1 sits on the slice itself, before the next statement.
    s=re.split(r'&&|;|\n|\)|`',seg)[0]
    return '2>&1' in s
for f in files:
    uses=[]; res={}
    for l in open(f,errors='replace'):
        if '"Bash"' not in l and 'tool_result' not in l: continue
        try: d=json.loads(l)
        except Exception: continue
        m=d.get('message')
        if not isinstance(m,dict) or not isinstance(m.get('content'),list): continue
        t=ts(d.get('timestamp',''))
        for b in m['content']:
            if not isinstance(b,dict): continue
            if b.get('type')=='tool_use' and b.get('name')=='Bash':
                if b['id'] in seen: continue
                seen.add(b['id'])
                if until and t and t>=until: continue
                c=b.get('input',{}).get('command','')
                if isinstance(c,str): uses.append((b['id'],c,t))
                if t:
                    tmin=t if not tmin or t<tmin else tmin; tmax=t if not tmax or t>tmax else tmax
            elif b.get('type')=='tool_result':
                cc=b.get('content')
                if isinstance(cc,list): cc='\n'.join(x.get('text','') for x in cc if isinstance(x,dict))
                res[b.get('tool_use_id')]=str(cc)
    for uid,cmd,t in uses:
        st['bash calls']+=1
        body=cmd.split('<<')[0]  # skip heredoc bodies
        segs=pipe_re.split(body)
        hit=False; piped_hit=False
        for j,s in enumerate(segs):
            for k,sub in enumerate(re.split(r'&&|;|\n|\$\(',s)):
                p=parse_slice(sub)
                if not p: continue
                piped=(j>0 and k==0)
                forms[('pipe ' if piped else 'nopipe ')+p[0]+' '+p[1]]+=1
                hit=True
                if not piped: continue
                piped_hit=True
                if slice_merges_stderr(s): st['Q1 slice with 2>&1 on it']+=1
                if p[2] is None: continue
                cut=nlines(res.get(uid,''))>=p[2]
                nsl['piped numeric slices']+=1
                nsl[p[0]+' piped numeric slices']+=1
                if cut:
                    nsl['piped numeric cut (lines >= N)']+=1
                    nsl[p[0]+' piped numeric cut (lines >= N)']+=1
        if hit: st['bash calls with head/tail']+=1
        if piped_hit:
            st['Q1 calls with a piped slice']+=1
            if CAPTURE_RE.search(body): st['Q1 calls capturing a slice in $( )']+=1
            if CAPTURE_MERGE_RE.search(body): st['Q1 captures with 2>&1 inside']+=1
            if GROUP_MERGE_RE.search(body): st['Q1 group or subshell with a slice, then 2>&1']+=1
        if 'pipefail' in cmd: st['Q1 calls setting pipefail']+=1
print('window',tmin,tmax)
for k in sorted(st): print(st[k],k)
print('-- forms (Q3)')
for k,v in forms.most_common(): print(v,k)
print(sum(forms.values()),'total head/tail uses')
print('-- piped numeric slices (Q5; numeric = -N, -n N, or no args)')
for k in sorted(nsl): print(nsl[k],k)
