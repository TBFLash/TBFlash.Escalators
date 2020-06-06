using SimAirport.Logging;
using Newtonsoft.Json;
using UnityEngine;
using System;
using AssetManagers;

namespace TBFlash.Escalators
{
    [JsonConverter(typeof(TBFlash_Converter<TBFlash_FastEscalator>))]
    public class TBFlash_FastEscalator : FloorPortal
    {
		private const float UDIR_TRAVERSAL_SPEED = 2.5f;
		private const float BDIR_TRAVERSAL_SPEED = 0.35f;
		public SpriteRenderer light0;
		public SpriteRenderer light1;
		public bool isBidirectional;
		public POConfig_Component ComponentConfig;
		private LeakyTokenBucket loadBalancer;
		private float speed;
		private readonly LeakyTokenBucket[] tokenBuckets = new LeakyTokenBucket[2];
		public Sprite lightSprite;

		public override void ISelectable_SelectedEnter(UITab tab, out Func<string> outDebug)
		{
			base.ISelectable_SelectedEnter(tab, out outDebug);
			outDebug = new Func<string>(inspect);
		}

		private string inspect()
		{
			return base.Inspect() + " \n\nloadBalancer.AvailabilityAmt01f=" + loadBalancer.AvailabilityAmt01f();
		}

		public override void NotifyEnroute()
		{
			loadBalancer.UseToken();
		}

		public override float UsageHeuristic(Cell start)
		{
			float num = Mathf.Pow(1f + tokenBuckets[Mathf.Clamp(GetSide(start), 0, 1)].SecondsTilTokenAvailable(), 1.5f) * 10f;
			return Mathf.Lerp(0f, speed * 1000f, Mathf.Pow(1f - loadBalancer.AvailabilityAmt01f(), 3f)) + num;
		}

		public override string Hover_OtherText()
		{
			if (!Debug.isDebugBuild)
			{
				return base.Hover_OtherText();
			}
			return string.Format("Heuristic[0]: {0}\nHeuristic[2]: {1}\nTokens: {2}\nTokenBucket[0] {3}\nTokenBucket[1] {4}\n", new object[]
			{
				UsageHeuristic(OnFootprint_Edge1[0]),
				UsageHeuristic(OnFootprint_Edge0[0]),
				loadBalancer.TokensRemaining,
				tokenBuckets[0],
				tokenBuckets[1]
			});
		}

		public float TraversalSpeed
		{
			get
			{
				return isBidirectional ? BDIR_TRAVERSAL_SPEED : UDIR_TRAVERSAL_SPEED;
			}
		}

        public override void AwakeComponent()
        {
            tokenBuckets[0] = new LeakyTokenBucket(3, 5);
            tokenBuckets[1] = new LeakyTokenBucket(3, 5);
            loadBalancer = new LeakyTokenBucket(35, 10);
            Game.current.tokenBuckets.Add(tokenBuckets[0]);
            Game.current.tokenBuckets.Add(tokenBuckets[1]);
            Game.current.tokenBuckets.Add(loadBalancer);
            speed = TraversalSpeed;
            base.AwakeComponent();

            TBFlash_Utils.TBFlashLogger(Log.FromPool("").WithCodepoint());

            GameObject go0 = new GameObject("light0go");
            light0 = go0.AddComponent<SpriteRenderer>();

            GameObject go1 = new GameObject("light1go");
            light1 = go1.AddComponent<SpriteRenderer>();
        }

		private void SetupColoredLights()
		{
			Texture2D text = TextureFromSprite(SpriteManager.Get("TBFlash_light"));
			light0.sprite = Sprite.Create(text, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
			Material sharedMaterial = MaterialManager.Get("Portals", light0.sprite.texture);
			light0.color = new Color(1f, 0f, 0f, 0.1f);
			light0.transform.position = placeableObj.GetMarker("light2").worldPosition;
			light0.transform.localScale = new Vector2(2.3f, 2.3f);
			light0.enabled = true;
			light0.sharedMaterial = sharedMaterial;
            light0.gameObject.layer = UILevelSelector.LevelToLayer(placeableObj.iprefab.level + 1);

			light1.sprite = Sprite.Create(text, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
			Material sharedMaterial2 = MaterialManager.Get("Portals", light1.sprite.texture);
			light1.color = new Color(0f, 1f, 0f, 0.1f);
			light1.transform.position = placeableObj.GetMarker("light1").worldPosition;
			light1.transform.localScale = new Vector2(2.3f, 2.3f);
			light1.enabled = true;
			light1.sharedMaterial = sharedMaterial2;
			light1.gameObject.layer = UILevelSelector.LevelToLayer(placeableObj.iprefab.level);

			DepthSort.SetupSingle(placeableObj, light0, -5f, false);
			DepthSort.SetupSingle(placeableObj, light1, -5f, false);
			SetupLightColors(currentDir == CurrentDir.One_To_Zero);
		}

		private static Texture2D TextureFromSprite(Sprite sprite)
		{
			if (sprite.rect.width != sprite.texture.width)
			{
				Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
				Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x, (int)sprite.textureRect.y, (int)sprite.textureRect.width, (int)sprite.textureRect.height);
				newText.SetPixels(newColors);
				newText.Apply();
				return newText;
			}
			else
            {
                return sprite.texture;
            }
        }

		protected void OnDestroy()
		{
			if (Game.isQuitting)
			{
				return;
			}
			Game.current.tokenBuckets.Remove(tokenBuckets[0]);
			Game.current.tokenBuckets.Remove(tokenBuckets[1]);
			Game.current.tokenBuckets.Remove(loadBalancer);
		}

		public bool IsEntry(Cell cell)
		{
			return (currentDir == FloorPortal.CurrentDir.BiDirectional && (CompareCell(OnFootprint_Edge0, cell) || CompareCell(OnFootprint_Edge1, cell))) || (currentDir == FloorPortal.CurrentDir.One_To_Zero && CompareCell(OnFootprint_Edge1, cell)) || (currentDir == FloorPortal.CurrentDir.Zero_To_One && CompareCell(OnFootprint_Edge0, cell));
		}

		public bool IsEnd(Cell cell)
		{
			return (currentDir == FloorPortal.CurrentDir.BiDirectional && (CompareCell(OnFootprint_Edge0, cell) || CompareCell(OnFootprint_Edge1, cell))) || (currentDir == FloorPortal.CurrentDir.One_To_Zero && CompareCell(OnFootprint_Edge0, cell)) || (currentDir == FloorPortal.CurrentDir.Zero_To_One && CompareCell(OnFootprint_Edge1, cell));
		}

		public override Cell RescuePosition()
		{
			if (currentDir == FloorPortal.CurrentDir.One_To_Zero)
			{
				return OFF_Footprint_Edge1[0];
			}
			if (currentDir == FloorPortal.CurrentDir.Zero_To_One)
			{
				return OFF_Footprint_Edge0[0];
			}
			return base.RescuePosition();
		}

		protected override void OnPlacement()
		{
			base.OnPlacement();
			placeableObj.prefab.iprefab.i18nNameKey = "TBFlash.FastEscalator.generic.name";
			TBFlash_Utils.TBFlashLogger(Log.FromPool("").WithCodepoint());
			if (light0 != null && light1 != null)
			{
				SetupColoredLights();
			}
		}

		public override void SetDirection(FloorPortal.CurrentDir dir)
		{
			TBFlash_Utils.TBFlashLogger(Log.FromPool("").WithCodepoint());
			if (isBidirectional)
			{
				dir = FloorPortal.CurrentDir.BiDirectional;
			}
			base.SetDirection(dir);
			SetupLightColors(dir == FloorPortal.CurrentDir.One_To_Zero);
		}

		private void SetupLightColors(bool flip)
		{
			TBFlash_Utils.TBFlashLogger(Log.FromPool("").WithCodepoint());
			if (light0 == null || light1 == null)
			{
				TBFlash_Utils.TBFlashLogger(Log.FromPool("").WithCodepoint());
				return;
			}
			if (flip)
			{
				light0.color = new Color(0f, 1f, 0f, 0.1f);
				light1.color = new Color(1f, 0f, 0f, 0.1f);
				return;
			}
			light0.color = new Color(1f, 0f, 0f, 0.1f);
			light1.color = new Color(0f, 1f, 0f, 0.1f);
		}

		private int GetSide(Cell c)
		{
			if (CompareCell(OnFootprint_Edge0, c))
			{
				return 0;
			}
			if (CompareCell(OnFootprint_Edge1, c))
			{
				return 1;
			}
			return -1;
		}

		public override bool GetTraversal(Cell start, out PortalTraversal traversal)
		{
			traversal = default;
			traversal.animate = (currentDir == FloorPortal.CurrentDir.BiDirectional);
			int side = GetSide(start);
			if (side < 0)
			{
				return false;
			}
			traversal.spd = TraversalSpeed;
			traversal.waitTime = tokenBuckets[side].UseToken();
			Cell cell = OppositeSide(start, false);
			if (cell != null)
			{
				traversal.PT0 = new TargetPosition(TargetPositions[(side == 0) ? 0 : 3], start.z);
				traversal.PT1 = new TargetPosition(TargetPositions[(side == 0) ? 1 : 2], cell.z);
				traversal.PT2 = new TargetPosition(TargetPositions[(side == 0) ? 2 : 1], cell.z);
				traversal.PT3 = new TargetPosition(TargetPositions[(side == 0) ? 3 : 0], cell.z);
				return true;
			}
			return false;
        }
	}
}
