﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugTools {
	public class DrawManager : MonoBehaviour {

		private static DrawManager instance;

		private Dictionary<string, IVariableDrawer> drawerDict;

		public DrawManager Initialise() {
			if (instance != null) {
				Destroy(this);
				return null;
			} else {
				instance = this;
				drawerDict = new Dictionary<string, IVariableDrawer>();
				return this;
			}
		}
		public static bool RegsiterDrawer(string drawerKey, IVariableDrawer drawer) {
			drawerKey = drawerKey.ToUpper();

			if (instance == null) {
				Console.Log(LogType.Error, "Draw Manager is null");
				return false;
			}

			if (instance.drawerDict == null) {
				Console.Log(LogType.Error, "Drawer Dictionary is null");
				return false;
			}

			if (instance.drawerDict.ContainsKey(drawerKey)) {
				Console.Log(LogType.Error, "Drawer Key must be unique");
				return false;
			}

			instance.drawerDict.Add(drawerKey, drawer);
			return instance.drawerDict.ContainsKey(drawerKey);
		}

		public static void DrawVariable(string drawerKey, object message) {
			drawerKey = drawerKey.ToUpper();

			if (instance.drawerDict.ContainsKey(drawerKey)) {
				instance.drawerDict[drawerKey].DrawVariable(drawerKey, message);
			}
		}
	}
}
