using BasicItemSync.Modules.Network.Server;
using SSMP.Networking.Packet;
using SSMP.Networking.Packet.Data;
using System;
using System.Collections.Generic;

namespace BasicItemSync.Modules.Network;

internal class ClientPacket : IPacketData
{
    public virtual bool IsReliable => true;
    public virtual bool DropReliableDataIfNewerExists => false;

    public virtual void ReadData(IPacket packet) { }
    public virtual void WriteData(IPacket packet) { }
}

internal class SendFlagPacket : ClientPacket
{
    public string Key = "";
    public string Name = "";
    public FlagType FlagType;

    public override void WriteData(IPacket packet)
    {
        packet.Write(Key);
        packet.Write(Name);
        packet.Write((int)FlagType);
    }

    public override void ReadData(IPacket packet)
    {
        Key = packet.ReadString();
        Name = packet.ReadString();
        FlagType = (FlagType)packet.ReadInt();
    }
}

internal class SendBoolItemPacket : SendFlagPacket
{
    public bool State = true;

    public override void WriteData(IPacket packet)
    {
        base.WriteData(packet);
        packet.Write(State);
    }

    public override void ReadData(IPacket packet)
    {
        base.ReadData(packet);
        State = packet.ReadBool();
    }
}

internal class SendUpgradeItemPacket : SendFlagPacket
{
    FlagType UpgradeType;

    public override void WriteData(IPacket packet)
    {
        base.WriteData(packet);
        packet.Write((int)UpgradeType);
    }

    public override void ReadData(IPacket packet)
    {
        base.ReadData(packet);
        UpgradeType = (FlagType)packet.ReadInt();
    }
}

internal class SendPersistentPacket : SendFlagPacket
{
    public string PersistentScene = "";
    public string PersistentObject = "";
    public override void WriteData(IPacket packet)
    {
        base.WriteData(packet);
        packet.Write(PersistentScene);
        packet.Write(PersistentObject);
    }
    public override void ReadData(IPacket packet)
    {
        base.ReadData(packet);
        PersistentScene = packet.ReadString();
        PersistentObject = packet.ReadString();
    }
}

internal class SendPersistentBoolPacket : SendPersistentPacket
{
    public bool State = true;
    public override void WriteData(IPacket packet)
    {
        base.WriteData(packet);
        packet.Write(State);
    }

    public override void ReadData(IPacket packet)
    {
        base.ReadData(packet);
        State = packet.ReadBool();
    }
}

internal class SendPersistentIntPacket : SendPersistentPacket
{
    public int State = 0;
    public override void WriteData(IPacket packet)
    {
        base.WriteData(packet);
        packet.Write(State);
    }

    public override void ReadData(IPacket packet)
    {
        base.ReadData(packet);
        State = packet.ReadInt();
    }
}

internal class SendIntItemPacket : SendFlagPacket
{
    public int Number = 0;

    public override void WriteData(IPacket packet)
    {
        base.WriteData(packet);
        packet.Write(Number);
    }

    public override void ReadData(IPacket packet)
    {
        base.ReadData(packet);
        Number = packet.ReadInt();
    }
}

internal class SendFloatItemPacket : SendFlagPacket
{
    public float Number = 0;

    public override void WriteData(IPacket packet)
    {
        base.WriteData(packet);
        packet.Write(Number);
    }

    public override void ReadData(IPacket packet)
    {
        base.ReadData(packet);
        Number = packet.ReadFloat();
    }
}

internal class SendCurrencyPacket : ClientPacket
{
    public short Rosaries;
    public short Shards;
    public override bool IsReliable => false;
    public override void WriteData(IPacket packet)
    {
        packet.Write(Rosaries);
        packet.Write(Shards);
    }

    public override void ReadData(IPacket packet)
    {
        Rosaries = packet.ReadShort();
        Shards = packet.ReadShort();
    }
}

internal class SettingsUpdatePacket : IPacketData
{
    public bool IsReliable => true;
    public bool DropReliableDataIfNewerExists => true;

    public SyncServerSettings Settings;

    public void WriteData(IPacket packet)
    {
        var props = Settings.ToValues();
        packet.Write(props.Count);
        foreach (var prop in props)
        {
            packet.Write(prop);
        }
    }

    public void ReadData(IPacket packet)
    {
        var len = packet.ReadInt();
        List<bool> values = [];
        for (var i = 0; i < len; i++)
        {
            values.Add(packet.ReadBool());
        }

        Settings = SyncServerSettings.PopulateFromValues(values);
    }
}

internal class SendQuestItemPacket : SendBoolItemPacket;

internal static class PacketInstantiate
{
    internal static IPacketData Instantiate(Packets packetID)
    {
        Log.LogDebug($"[PACKETS] Received {packetID}");
        return packetID switch
        {
            Packets.BoolPlayerData => new PacketDataCollection<SendBoolItemPacket>(),
            Packets.IntPlayerData => new PacketDataCollection<SendIntItemPacket>(),
            Packets.FloatPlayerData => new PacketDataCollection<SendFloatItemPacket>(),
            Packets.Currency => new PacketDataCollection<SendCurrencyPacket>(),
            Packets.Quest => new PacketDataCollection<SendBoolItemPacket>(),
            Packets.Tool => new PacketDataCollection<SendBoolItemPacket>(),
            Packets.Upgrade => new PacketDataCollection<SendFlagPacket>(),
            Packets.Collectable => new PacketDataCollection<SendPersistentBoolPacket>(),
            Packets.PersistentBool => new PacketDataCollection<SendPersistentBoolPacket>(),
            Packets.PersistentInt => new PacketDataCollection<SendPersistentIntPacket>(),
            Packets.Settings => new SettingsUpdatePacket(),
            _ => new ErrorThrowingPacket(packetID, true)
        };
    }
}

internal class ErrorThrowingPacket : ClientPacket
{
    public ErrorThrowingPacket(Packets id, bool server)
    {
        Log.LogError(id.ToString(), server);
        throw new NotImplementedException(id.ToString());
    }
}