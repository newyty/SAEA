﻿/****************************************************************************
*项目名称：SAEA.MQTT.Core.Protocol
*CLR 版本：4.0.30319.42000
*机器名称：WENLI-PC
*命名空间：SAEA.MQTT.Core.Protocol
*类 名 称：ReconnectionResult
*版 本 号：V1.0.0.0
*创建人： yswenli
*电子邮箱：wenguoli_520@qq.com
*创建时间：2019/1/16 9:38:55
*描述：
*=====================================================================
*修改时间：2019/1/16 9:38:55
*修 改 人： yswenli
*版 本 号： V1.0.0.0
*描    述：
*****************************************************************************/

namespace SAEA.MQTT.Core.Protocol
{
    public enum ReconnectionResult
    {
        StillConnected,
        Reconnected,
        NotConnected
    }
}
