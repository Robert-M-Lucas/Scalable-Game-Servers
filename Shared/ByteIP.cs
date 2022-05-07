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

    public static ByteIP BytesToIP(byte[] bytes) {
        ByteIP byteIP = new ByteIP();
        
        byteIP.IP = ArrayExtentions.Slice(bytes, 0, 4);
        byteIP.Port = ArrayExtentions.Slice(bytes, 4, 6);

        byteIP.iPort = ((uint) byteIP.Port[0]) + ((uint) (byteIP.Port[1]<<8));

        for (int i = 0; i < 4; i++){
            byteIP.strIP += ((uint) bytes[i]);
            if (i == 3) { break; }
            byteIP.strIP += ".";
        }

        return byteIP;
    }

    public override string ToString() {
        return $"{strIP}:{iPort}";
    }
}