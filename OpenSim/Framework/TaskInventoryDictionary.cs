/*
 * Copyright (c) Contributors, http://whitecore-sim.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 * For an explanation of the license of each contributor and the content it 
 * covers please see the Licenses directory.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the WhiteCore-Sim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using OpenMetaverse;

namespace OpenSim.Framework
{
    /// <summary>
    ///     A dictionary for task inventory.
    ///
    /// This class is not thread safe.  Callers must synchronize on Dictionary methods or Clone() this object before
    /// iterating over it.
    /// </summary>
    public class TaskInventoryDictionary : Dictionary<UUID, TaskInventoryItem>, ICloneable, IXmlSerializable
    {
        private static XmlSerializer tiiSerializer = new XmlSerializer(typeof(TaskInventoryItem));

        #region ICloneable Members

        public Object Clone()
        {
            TaskInventoryDictionary clone = new TaskInventoryDictionary();

            lock (this)
            {
                foreach (UUID uuid in Keys)
                {
                    clone.Add(uuid, (TaskInventoryItem)this[uuid].Clone());
                }
            }

            return clone;
        }

        #endregion

        // The alternative of simply serializing the list doesn't appear to work on mono, since
        // we get a
        //
        // System.TypeInitializationException: An exception was thrown by the type initializer for OpenSim.Framework.TaskInventoryDictionary ---> System.ArgumentOutOfRangeException: < 0
        // Parameter name: length
        //   at System.String.Substring (Int32 startIndex, Int32 length) [0x00088] in /build/buildd/mono-1.2.4/mcs/class/corlib/System/String.cs:381
        //   at System.Xml.Serialization.TypeTranslator.GetTypeData (System.Type runtimeType, System.String xmlDataType) [0x001f6] in /build/buildd/mono-1.2.4/mcs/class/System.XML/System.Xml.Serialization/TypeTranslator.cs:217
        // see IXmlSerializable

        #region IXmlSerializable Members

        public XmlSchema GetSchema()
        {
            return null;
        }

        // see IXmlSerializable
        public void ReadXml(XmlReader reader)
        {
            if (!reader.IsEmptyElement)
            {
                reader.Read();
                while (tiiSerializer.CanDeserialize(reader))
                {
                    TaskInventoryItem item = (TaskInventoryItem)tiiSerializer.Deserialize(reader);
                    Add(item.ItemID, item);
                }
            }

            // For some .net implementations, this last read is necessary so that we advance beyond the end tag
            // of the element wrapping this object so that the rest of the serialization can complete normally.
            reader.Read();
        }

        // see IXmlSerializable
        public void WriteXml(XmlWriter writer)
        {
            lock (this)
            {
                foreach (TaskInventoryItem item in Values)
                {
                    tiiSerializer.Serialize(writer, item);
                }
            }
        }

        #endregion

        // see ICloneable
    }
}