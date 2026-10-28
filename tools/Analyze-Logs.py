from pathlib import Path
from collections import Counter
import json,sys
root=Path(sys.argv[1]).resolve()
files=sorted(root.rglob("*.jsonl")) if root.is_dir() else [root]
if len(files)>1000:raise SystemExit("Too many files; select a date or one session")
if sum(f.stat().st_size for f in files)>256*1024*1024:raise SystemExit("Over 256 MiB; select one session")
seen=set();kinds=Counter();changed=Counter();states=Counter();markers=[];bad=0;sessions=set();input_by_session={};events=[]
for path in files:
    with path.open(encoding="utf-8") as stream:
        for line in stream:
            try:
                row=json.loads(line);identity=(row["session"],row["sequence"])
                if identity in seen:continue
                seen.add(identity);sessions.add(row["session"]);kind=row["kind"];kinds[kind]+=1
                data=row.get("data",{})
                if kind=="event":events.append((row["utc"],data))
                if kind=="marker":markers.append((row["utc"],data.get("kind")))
                if kind=="snapshot":
                    for index,value in data.get("numericChanges",[]):changed["count" if index==0 else "enemy_field_"+str((index-1)%13)]+=1
                    if data.get("full"):input_by_session[row["session"]]={}
                    if "input" in data.get("ui",{}):input_by_session[row["session"]]=data["ui"]["input"]
                    inp=input_by_session.get(row["session"],{})
                    states[str((inp.get("visible"),inp.get("enabled"),inp.get("focused"),inp.get("inputLength")))]+=1
            except (ValueError,KeyError,TypeError):bad+=1
print(json.dumps({"files":len(files),"sessions":len(sessions),"records":dict(kinds),"invalid_lines":bad,"changed_numeric_fields":dict(changed),"input_state_change_samples":dict(states),"markers":markers,"events":events,"note":"Jammer end is inferred from a late cast disappearing; four-second pause is user-selected. Other unknown fields remain uninterpreted."},ensure_ascii=False,indent=2))
