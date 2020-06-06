using Newtonsoft.Json;
using System;
using UnityEngine;
using SimAirport.Logging;

namespace TBFlash.Escalators
{
	public class TBFlash_Converter<T> : JsonConverter
	{
		public Type usingType;

		public TBFlash_Converter()
		{
			usingType = typeof(T);
			TBFlash_Utils.TBFlashLogger(Log.FromPool($"Created TBFlash_Converter of Type: {usingType}").WithCodepoint());
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			IPrefab prefab = null;
			MonoBehaviour monoBehaviour = value as MonoBehaviour;
			if (value is IJsonSaveable jsonSaveable)
			{
				prefab = jsonSaveable.iprefab;
			}
			else if (monoBehaviour != null)
			{
				Prefab component = monoBehaviour.GetComponent<Prefab>();
				if (component != null)
				{
					prefab = component.iprefab;
				}
			}
			if (ShouldSerializeIPrefab(prefab))
			{
				serializer.Serialize(writer, prefab.guid.ToString());
				return;
			}
			writer.WriteComment("nulled destroyed MIA");
			serializer.Serialize(writer, null);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			string text = serializer.Deserialize<string>(reader);
			if (text == null)
			{
				return null;
			}
			IPrefab prefab = GUID.Fetch(int.Parse(text));
			if (prefab == null)
			{
				TBFlash_Utils.TBFlashLogger(Log.FromPool(string.Format("GUID.Fetch {0} was NULL // NOT FOUND", text)).WithCodepoint());
				return null;
			}
			if (prefab.zone != null)
			{
				return prefab.zone;
			}
			if (prefab.agent != null)
			{
				return prefab.agent;
			}
			if (prefab.iPoolable != null)
			{
				return prefab.iPoolable;
			}
			if (prefab.prefab != null)
			{
				Component component = prefab.prefab.gameObject.GetComponent(usingType);
				if (component == null)
				{
					TBFlash_Utils.TBFlashLogger(Log.FromPool("NULL GetComponent<T> Lookup").WithCodepoint());
				}
				return component;
			}
			TBFlash_Utils.TBFlashLogger(Log.FromPool("NULL GetComponent<T> Lookup").WithCodepoint());
			return null;
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == usingType;
		}

		private static bool ShouldSerializeIPrefab(IPrefab i)
		{
			return i?.isPooled == false && i.guid != GUID.None && (i.prefab?.commitable != false);
		}
	}
}
