using System;
using System.Text;
using PCSC;
using PCSC.Exceptions;
using PCSC.Utils;

namespace crs
{
    class MainClass
    {
        static void CheckErr(SCardError err)
        {
            if (err != SCardError.Success)
                throw new PCSCException(err,
                    SCardHelper.StringifyError(err));
        }
        static void Main(string[] args)
        {
            try
            {
                // Establish SCard context
                SCardContext hContext = new SCardContext();
                hContext.Establish(SCardScope.System);

                // Retrieve the list of Smartcard readers
                string[] szReaders = hContext.GetReaders();
                if (szReaders.Length <= 0)
                    throw new PCSCException(SCardError.NoReadersAvailable,
                        "Could not find any Smartcard reader.");

                Console.WriteLine("reader name: " + szReaders[0]);

                // Create a reader object using the existing context
                SCardReader reader = new SCardReader(hContext);

                // Connect to the card
                SCardError err = reader.Connect(szReaders[0],
                    SCardShareMode.Shared,
                    SCardProtocol.T0 | SCardProtocol.T1);
                CheckErr(err);

                IntPtr pioSendPci;
                switch (reader.ActiveProtocol)
                {
                    case SCardProtocol.T0:
                        pioSendPci = SCardPCI.T0;
                        break;
                    case SCardProtocol.T1:
                        pioSendPci = SCardPCI.T1;
                        break;
                    default:
                        throw new PCSCException(SCardError.ProtocolMismatch,
                            "Protocol not supported: "
                            + reader.ActiveProtocol.ToString());
                }

                byte[] pbRecvBuffer = new byte[256];

                // Send SELECT_MF command
                byte[] cmd1 = { 0x00, 0xA4, 0x00, 0x00, 0x02, 0x3F, 0x00 };
                err = reader.Transmit(pioSendPci, cmd1, ref pbRecvBuffer);
                CheckErr(err);

                Console.Write("response: ");
                for (int i = 0; i < pbRecvBuffer.Length; i++)
                    Console.Write("{0:X2} ", pbRecvBuffer[i]);
                Console.WriteLine();

                pbRecvBuffer = new byte[256];

                // Send SELECT_DF1 command
                byte[] cmd2 = { 0x00, 0xA4, 0x00, 0x00, 0x02, 0x11, 0x00 };
                err = reader.Transmit(pioSendPci, cmd2, ref pbRecvBuffer);
                CheckErr(err);

                Console.Write("response: ");
                for (int i = 0; i < pbRecvBuffer.Length; i++)
                    Console.Write("{0:X2} ", pbRecvBuffer[i]);
                Console.WriteLine();

                pbRecvBuffer = new byte[256];

                // Send SELECT_EF_PERS command
                byte[] cmd3 = { 0x00, 0xA4, 0x00, 0x00, 0x02, 0x11, 0x02 };
                err = reader.Transmit(pioSendPci, cmd3, ref pbRecvBuffer);
                CheckErr(err);

                Console.Write("response: ");
                for (int i = 0; i < pbRecvBuffer.Length; i++)
                    Console.Write("{0:X2} ", pbRecvBuffer[i]);
                Console.WriteLine();

                pbRecvBuffer = new byte[256];

                // Send READ_BIN command
                byte[] cmd4 = { 0x00, 0xB0, 0x00, 0x00, 0x00 };
                err = reader.Transmit(pioSendPci, cmd4, ref pbRecvBuffer);
                CheckErr(err);

                Console.Write("response: ");
                for (int i = 0; i < pbRecvBuffer.Length; i++)
                    Console.Write("{0:X2} ", pbRecvBuffer[i]);
                Console.WriteLine();
                Console.WriteLine(Encoding.UTF8.GetString(pbRecvBuffer));

                /*
                pbRecvBuffer = new byte[256];

                // Send SELECT_DF0 command
                byte[] cmd2 = { 0x00, 0xA4, 0x00, 0x00, 0x02, 0x10, 0x00 };
                err = reader.Transmit(pioSendPci, cmd2, ref pbRecvBuffer);
                CheckErr(err);

                Console.Write("response: ");
                for (int i = 0; i < pbRecvBuffer.Length; i++)
                    Console.Write("{0:X2} ", pbRecvBuffer[i]);
                Console.WriteLine();

                pbRecvBuffer = new byte[256];

                // Send SELECT_EF_ID_Carta command
                byte[] cmd3 = { 0x00, 0xA4, 0x00, 0x00, 0x02, 0x10, 0x03 };
                err = reader.Transmit(pioSendPci, cmd3, ref pbRecvBuffer);
                CheckErr(err);

                Console.Write("response: ");
                for (int i = 0; i < pbRecvBuffer.Length; i++)
                    Console.Write("{0:X2} ", pbRecvBuffer[i]);
                Console.WriteLine();

                pbRecvBuffer = new byte[256];

                // Send READ_BIN command
                byte[] cmd4 = { 0x00, 0xB0, 0x00, 0x00, 0x00 };
                err = reader.Transmit(pioSendPci, cmd4, ref pbRecvBuffer);
                CheckErr(err);

                Console.Write("response: ");
                for (int i = 0; i < pbRecvBuffer.Length; i++)
                    Console.Write("{0:X2} ", pbRecvBuffer[i]);
                Console.WriteLine();
                Console.WriteLine(Encoding.UTF8.GetString(pbRecvBuffer));
                */

                hContext.Release();
            }
            catch (PCSCException ex)
            {
                Console.WriteLine("Ouch: "
                    + ex.Message
                    + " (" + ex.SCardError.ToString() + ")");
            }
        }
    }
}
