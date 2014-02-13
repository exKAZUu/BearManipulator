using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace BearManipulator {
	public class Program {
		private const int LocalPort = 9091;
		private const int RemotePort = 9090;
		private static UdpClient _udpClient;
		private static IPEndPoint _udpSendPoint;
		private static readonly List<int> LeftArmUp;
		private static readonly List<int> RightArmUp;
		private static readonly List<int> LeftLegUp;
		private static readonly List<int> RightLegUp;
		private static readonly List<int> Jump;

		static Program() {
			LeftArmUp = new List<int> {
				0, 0, 0, 0, 0, 0, 30000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
			};
			RightArmUp = new List<int> {
				0, 0, -30000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
			};
			LeftLegUp = new List<int> {
				0, 0, 0, 30000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 30000, 0
			};
			RightLegUp = new List<int> {
				0, 0, 0, 0, 0, 0, 0, 0, 0, 30000, 0, 0, 0, 0, 0, 0, 0, 0, -10000, 0
			};
			Jump = new List<int> {
				0, 0, 0, 0, 0, 0, 0, 0, 30000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 30000, 0
			};
		}

		private static void Add(List<int> list1, List<int> list2) {
			for (int i = 0; i < list1.Count; i++) {
				list1[i] += list2[i];
			}
		}

		private static void Main(string[] args) {
			var address = Dns.GetHostAddresses("192.168.91.57")[0];
			_udpClient = new UdpClient(LocalPort);
			_udpSendPoint = new IPEndPoint(address, RemotePort);

			Initialize();

			var command = new List<int> {
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
			};

			var cmds = args.FirstOrDefault() ?? "rau";
			if (cmds.Contains("lau")) {
				Console.WriteLine("Left Arm Up");
				Add(command, LeftArmUp);
			}
			if (cmds.Contains("rau")) {
				Console.WriteLine("Right Arm Up");
				Add(command, RightArmUp);
			}
			if (cmds.Contains("llu")) {
				Console.WriteLine("Left Leg Up");
				Add(command, LeftLegUp);
			}
			if (cmds.Contains("rlu")) {
				Console.WriteLine("Right Leg Up");
				Add(command, RightLegUp);
			}
			if (cmds.Contains("jump")) {
				Console.WriteLine("Jump");
				Add(command, Jump);
			}
			SendPose(command);
		}

		private static void SendPose(IEnumerable<int> motors) {
			var packet = new byte[1000];
			int p = 0;
			packet[p++] = 0x00;
			packet[p++] = 0x02;
			foreach (var motor in motors) {
				//v += (int)((Calib)calibs[motorMap[i]]).udCalib.Value;
				packet[p++] = (byte)((motor >> 8) & 0xFF);
				packet[p++] = (byte)(motor & 0xFF);
			}
			_udpClient.Send(packet, p, _udpSendPoint);
		}

		private static void Initialize() {
			{
				// OFF
				var packet = new byte[1000];
				var p = 0;
				packet[p++] = 0x00;
				packet[p++] = 0x21;
				//            packet[p++] = (byte)((interval >> 8) & 0xFF);
				//            packet[p++] = (byte)(interval & 0xFF);

				_udpClient.Send(packet, p, _udpSendPoint);
			}
			{
				// NO
				var packet = new byte[1000];
				var p = 0;
				packet[p++] = 0x00;
				packet[p++] = 0x22; //PWM開始コマンド
				//            packet[p++] = (byte)((interval >> 8) & 0xFF);
				//            packet[p++] = (byte)(interval & 0xFF);

				_udpClient.Send(packet, p, _udpSendPoint);

				p = 0;
				packet[p++] = 0x00;
				packet[p++] = 0x24; //サーボ制御開始コマンド
				_udpClient.Send(packet, p, _udpSendPoint);
			}
		}

		public class PoseData : IComparable {
			public PoseData() {
				Values = new List<int>();
			}

			public int Time { get; set; }
			public List<int> Values { get; set; }

			public int CompareTo(object o) {
				return Time - ((PoseData)o).Time;
			}
		}
	}
}