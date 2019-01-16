﻿/****************************************************************************
*项目名称：SAEA.MQTT.Common.Serializer
*CLR 版本：4.0.30319.42000
*机器名称：WENLI-PC
*命名空间：SAEA.MQTT.Common.Serializer
*类 名 称：MqttPacketReader
*版 本 号：V1.0.0.0
*创建人： yswenli
*电子邮箱：wenguoli_520@qq.com
*创建时间：2019/1/15 15:32:34
*描述：
*=====================================================================
*修改时间：2019/1/15 15:32:34
*修 改 人： yswenli
*版 本 号： V1.0.0.0
*描    述：
*****************************************************************************/
using SAEA.MQTT.Exceptions;
using SAEA.MQTT.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SAEA.MQTT.Common.Serializer
{
    public static class MqttPacketReader
    {
        public static async Task<MqttFixedHeader> ReadFixedHeaderAsync(IMqttChannel channel, byte[] fixedHeaderBuffer, byte[] singleByteBuffer, CancellationToken cancellationToken)
        {
            // The MQTT fixed header contains 1 byte of flags and at least 1 byte for the remaining data length.
            // So in all cases at least 2 bytes must be read for a complete MQTT packet.
            // async/await is used here because the next packet is received in a couple of minutes so the performance
            // impact is acceptable according to a useless waiting thread.
            var buffer = fixedHeaderBuffer;
            var totalBytesRead = 0;

            while (totalBytesRead < buffer.Length)
            {
                var bytesRead = await channel.ReadAsync(buffer, totalBytesRead, buffer.Length - totalBytesRead, cancellationToken).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();
                ExceptionHelper.ThrowIfGracefulSocketClose(bytesRead);

                totalBytesRead += bytesRead;
            }

            var hasRemainingLength = buffer[1] != 0;
            if (!hasRemainingLength)
            {
                return new MqttFixedHeader(buffer[0], 0);
            }


            // Here the async/await pattern is not used becuase the overhead of context switches
            // is too big for reading 1 byte in a row. We expect that the remaining data was sent
            // directly after the initial bytes. If the client disconnects just in this moment we
            // will get an exception anyway.
            var bodyLength = ReadBodyLength(channel, buffer[1], singleByteBuffer, cancellationToken);


            return new MqttFixedHeader(buffer[0], bodyLength);
        }

        private static int ReadBodyLength(IMqttChannel channel, byte initialEncodedByte, byte[] singleByteBuffer, CancellationToken cancellationToken)
        {
            var offset = 0;
            var multiplier = 128;
            var value = initialEncodedByte & 127;
            int encodedByte = initialEncodedByte;

            while ((encodedByte & 128) != 0)
            {
                offset++;
                if (offset > 3)
                {
                    throw new MqttProtocolViolationException("Remaining length is invalid.");
                }

                cancellationToken.ThrowIfCancellationRequested();

                encodedByte = ReadByte(channel, singleByteBuffer, cancellationToken);

                value += (byte)(encodedByte & 127) * multiplier;
                multiplier *= 128;
            }

            return value;
        }

        private static byte ReadByte(IMqttChannel channel, byte[] singleByteBuffer, CancellationToken cancellationToken)
        {
            var readCount = channel.ReadAsync(singleByteBuffer, 0, 1, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();

            cancellationToken.ThrowIfCancellationRequested();

            if (readCount <= 0)
            {
                ExceptionHelper.ThrowGracefulSocketClose();
            }

            return singleByteBuffer[0];
        }
    }
}
