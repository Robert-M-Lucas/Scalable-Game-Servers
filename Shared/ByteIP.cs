namespace Shared;

public class ByteIP {
    public byte[] IP = new byte[4];
    public byte[] Port = new byte[2];
    public string strIP = "";
    public uint iPort = 0;

    private ByteIP(){}

    public static ByteIP StringToIP(string sIP, uint iPort) {
        ByteIP byteIP = new ByteIP();
        
        byteIP.strIP = sIP;
        byteIP.iPort = iPort;

        string[] split = sIP.Split('.');

        for (int i = 0; i < 4; i++) {
            byteIP.IP[i] = (byte) uint.Parse(split[i]);
        }

        byteIP.Port[0] = (byte) iPort;
        byteIP.Port[1] = (byte) (iPort>>8);

        return byteIP;
    }
}