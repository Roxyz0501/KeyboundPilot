from pathlib import Path
import hashlib, json, re, struct, zipfile
r=Path(__file__).resolve().parents[1]
p=r/"KeyboundPilot"
out=p/"bin/Release"
m=json.loads((out/"KeyboundPilot.json").read_text(encoding="utf-8-sig"))
assert m["Author"]=="Roxyz0501" and m["AssemblyVersion"]=="1.0.16.16"
assert m["RepoUrl"]=="https://github.com/Roxyz0501/KeyboundPilot"
assert m["IconUrl"]=="https://raw.githubusercontent.com/Roxyz0501/KeyboundPilot/main/KeyboundPilot/images/icon.png"
loc=(p/"Loc.cs").read_text(encoding="utf-8")
rows=re.findall(r'^        \["([^"\n]+)"\] = (\[.*\]),$',loc,re.M)
assert len(rows)==152
for key,value in rows:
    values=json.loads(value)
    assert len(values)==7 and all(values),key
canonical=r.parent/"templates/Dalamud/StandardPluginTabs.cs"
if canonical.exists():assert (p/"StandardPluginTabs.cs").read_bytes()==canonical.read_bytes()
icon=(p/"images/icon.png").read_bytes()
assert icon[:8]==b"\x89PNG\r\n\x1a\n" and struct.unpack(">II",icon[16:24])==(512,512)
a=r/"artifacts";a.mkdir(exist_ok=True)
z=a/("KeyboundPilot-"+m["AssemblyVersion"]+".zip")
files={out/"KeyboundPilot.dll":"KeyboundPilot.dll",out/"KeyboundPilot.json":"KeyboundPilot.json",out/"KeyboundPilot.deps.json":"KeyboundPilot.deps.json",r/"LICENSE":"LICENSE",r/"NOTICE":"NOTICE",r/"README.md":"README.md",r/"VALIDATION.md":"VALIDATION.md",p/"images/icon.png":"images/icon.png"}
with zipfile.ZipFile(z,"w",zipfile.ZIP_DEFLATED) as archive:
    for file,name in files.items():archive.write(file,name)
with zipfile.ZipFile(z) as archive:
    assert archive.testzip() is None
    assert json.loads(archive.read("KeyboundPilot.json"))==m
    assert archive.read("KeyboundPilot.dll")==files.keys().__iter__().__next__().read_bytes()
digest=hashlib.sha256(z.read_bytes()).hexdigest()
(a/(z.name+".sha256")).write_text(digest+"  "+z.name+"\n",encoding="utf-8")
metadata={"version":m["AssemblyVersion"],"asset":z.name,"bytes":z.stat().st_size,"sha256":digest,"RepoUrl":m["RepoUrl"],"IconUrl":m["IconUrl"],"prerelease":True,"live_game_verified":False,"tests":572,"localization_keys":152,"languages":7}
(r/"release-metadata.json").write_text(json.dumps(metadata,indent=2)+"\n",encoding="utf-8")
print(json.dumps(metadata))
