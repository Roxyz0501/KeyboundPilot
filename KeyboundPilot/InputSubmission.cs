using FFXIVClientStructs.FFXIV.Component.GUI;
namespace KeyboundPilot;

internal static unsafe class InputSubmission
{
    // Only invoke the callback registered by this exact input component and owner.
    // This is synchronous and independent of keyboard polling/window activation.
    public static bool TryInvoke(AtkComponentTextInput* input,AtkUnitBase* owner,out InputCallbackResult result)
    {
        result=InputCallbackResult.None;
        if(input==null || owner==null || !input->Enabled || !input->IsActive || input->OwnerAddon!=owner
            || input->Callback==null || (byte*)input->RawString.StringPtr==null || (byte*)input->EvaluatedString.StringPtr==null)return false;
        result=input->Callback(owner,InputCallbackType.Enter,input->RawString.StringPtr,input->EvaluatedString.StringPtr,input->CallbackEventKind);
        return true;
    }
}

