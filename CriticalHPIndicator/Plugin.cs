using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.Interop;

namespace CriticalHPIndicator;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    private readonly byte[] criticalColor = [255, 32, 32];
    private readonly byte[] defaultColor = [100, 100, 100];

    public Plugin()
    {
        AddonLifecycle.RegisterListener(AddonEvent.PostDraw, "_PartyList", OnPartyListDraw);
        AddonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, "_PartyList", OnPartyListDraw);
    }

    public void Dispose()
    {
        AddonLifecycle.UnregisterListener(OnPartyListDraw);
    }

    private unsafe void SetBarColor(AtkComponentGaugeBar* hpGaugeBar, byte[] color)
    {
        var barNode = hpGaugeBar->GetNineGridNodeById(11);
        if (barNode != null)
        {
            barNode->MultiplyRed = color[0];
            barNode->MultiplyGreen = color[1];
            barNode->MultiplyBlue = color[2];   
        }
        
        var glowNode = hpGaugeBar->GetImageNodeById(12);
        if (glowNode != null)
        {
            glowNode->MultiplyRed = color[0];
            glowNode->MultiplyGreen = color[1];
            glowNode->MultiplyBlue = color[2];   
        }
    }

    private static unsafe bool IsCriticalHealth(HudPartyMember partyMember)
    {
        var currentHealth = partyMember.Object->Health;
        var maxHealth = partyMember.Object->MaxHealth;
        return currentHealth <= maxHealth * 0.1;
    }
    
    private unsafe void OnPartyListDraw(AddonEvent type, AddonArgs args)
    {
        var addon = (AddonPartyList*)(args.Addon.Address);
        var partyMembers = AgentHUD.Instance()->PartyMembers;

        var i = 0;
        foreach (var partyMember in partyMembers)
        {
            if (partyMember.EntityId is 0xE0000000) continue;
            var color = IsCriticalHealth(partyMember) ? criticalColor : defaultColor;
            var partyListMember = partyMember.ContentId is 0
                                      ? addon->TrustMembers.GetPointer(i)
                                      : addon->PartyMembers.GetPointer(i);
            var hpGaugeBar = partyListMember->HPGaugeBar;
            if (hpGaugeBar == null) continue;
            SetBarColor(hpGaugeBar, color);
            i++;
        }
    }
}
