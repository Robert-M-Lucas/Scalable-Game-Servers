namespace Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class ClientConnectRequestPacket {
    public const int UID = 0;
    public int RID;
    public string Name;
    public string Version;
    public ClientConnectRequestPacket(Packet packet){
        Name = ASCIIEncoding.ASCII.GetString(packet.contents[0]);
        Version = ASCIIEncoding.ASCII.GetString(packet.contents[1]);
    }

    public static byte[] Build(int _RID, string _Name, string _Version) {
        List<byte[]> contents = new List<byte[]>();
        contents.Add(ASCIIEncoding.ASCII.GetBytes(_Name));
        contents.Add(ASCIIEncoding.ASCII.GetBytes(_Version));
        return PacketManager.Build(UID, contents);
    }
}