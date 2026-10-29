using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Component.GUI;
using InteropGenerator.Runtime;
using KeyboundPilot;
unsafe partial class Program
{
 static int submitCalls;static bool submitArgs;static nint expectedOwner;
 [UnmanagedCallersOnly]
 static InputCallbackResult FakeSubmit(AtkUnitBase* owner,InputCallbackType type,CStringPointer raw,CStringPointer evaluated,int kind)
 {
  submitCalls++;submitArgs=(nint)owner==expectedOwner && type==InputCallbackType.Enter && kind==37 && ((byte*)raw)[0]==65 && ((byte*)evaluated)[0]==66;
  return InputCallbackResult.ClearText;
 }
 static void CheckInputSubmission()
 {
  AtkUnitBase owner=default,other=default;AtkComponentTextInput input=default;
  byte* raw=stackalloc byte[]{65,0};byte* evaluated=stackalloc byte[]{66,0};
  expectedOwner=(nint)(&owner);input.OwnerAddon=&owner;input.Enabled=true;input.IsActive=true;input.Callback=&FakeSubmit;input.CallbackEventKind=37;input.RawString.StringPtr=raw;input.EvaluatedString.StringPtr=evaluated;
  if(!InputSubmission.TryInvoke(&input,&owner,out var result) || result!=InputCallbackResult.ClearText || submitCalls!=1 || !submitArgs)throw new Exception("Input callback ABI/arguments/result mismatch");
  if(InputSubmission.TryInvoke(&input,&other,out _) || InputSubmission.TryInvoke(null,&owner,out _))throw new Exception("Wrong owner or null input accepted");
  input.Enabled=false;if(InputSubmission.TryInvoke(&input,&owner,out _))throw new Exception("Disabled input accepted");input.Enabled=true;
  input.IsActive=false;if(InputSubmission.TryInvoke(&input,&owner,out _))throw new Exception("Inactive component accepted");input.IsActive=true;
  input.Callback=null;if(InputSubmission.TryInvoke(&input,&owner,out _) || submitCalls!=1)throw new Exception("Missing callback accepted");
  Console.WriteLine("PASS: native input callback ABI, owner, Enter kind, raw/evaluated pointers, cleanup result and invalid-input rejection; no game/OS calls");
 }
}
