﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Raven.Bundles.Compression.Streams
{
	internal class StreamReaderWithUnread
	{
		public readonly Stream Stream;
		private readonly Stack<Buffer> unreadBuffer = new Stack<Buffer>();

		public StreamReaderWithUnread(Stream stream)
		{
			Stream = stream;
		}

		public int Read(byte[] buffer, int start, int length)
		{
			if (unreadBuffer.Count != 0)
			{
				var next = unreadBuffer.Pop();
				if (next.Length > length)
				{
					Array.Copy(next.Data, next.Start, buffer, start, length);
					unreadBuffer.Push(next.Skip(length));
					return length;
				}
				
				Array.Copy(next.Data, next.Start, buffer, start, next.Length);
				return next.Length;
			}

			return Stream.Read(buffer, start, length);
		}

		public void Unread(IEnumerable<byte> data)
		{
			unreadBuffer.Push(new Buffer(data));
		}

		public void Close()
		{
			Stream.Close();
		}

		private struct Buffer
		{
			public readonly byte[] Data;
			public readonly int Start;
			public readonly int Length;

			public Buffer(IEnumerable<byte> data)
			{
				Data = data.ToArray();
				Start = 0;
				Length = Data.Length;
			}

			public Buffer(byte[] data, int start, int length)
			{
				Data = data;
				Start = start;
				Length = length;
			}

			public Buffer Skip(int count)
			{
				return new Buffer(Data, Start + count, Length - count);
			}
		}
	}
}