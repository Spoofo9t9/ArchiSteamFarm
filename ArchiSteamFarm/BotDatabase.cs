﻿/*
    _                _      _  ____   _                           _____
   / \    _ __  ___ | |__  (_)/ ___| | |_  ___   __ _  _ __ ___  |  ___|__ _  _ __  _ __ ___
  / _ \  | '__|/ __|| '_ \ | |\___ \ | __|/ _ \ / _` || '_ ` _ \ | |_  / _` || '__|| '_ ` _ \
 / ___ \ | |  | (__ | | | || | ___) || |_|  __/| (_| || | | | | ||  _|| (_| || |   | | | | | |
/_/   \_\|_|   \___||_| |_||_||____/  \__|\___| \__,_||_| |_| |_||_|   \__,_||_|   |_| |_| |_|

 Copyright 2015-2016 Łukasz "JustArchi" Domeradzki
 Contact: JustArchi@JustArchi.net

 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at

 http://www.apache.org/licenses/LICENSE-2.0
					
 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.

*/

using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace ArchiSteamFarm {
	internal sealed class BotDatabase {
		[JsonProperty]
		private string _LoginKey;

		internal string LoginKey {
			get {
				return _LoginKey;
			}

			set {
				if (_LoginKey == value) {
					return;
				}

				_LoginKey = value;
				Save();
			}
		}

		[JsonProperty]
		private MobileAuthenticator _MobileAuthenticator;

		internal MobileAuthenticator MobileAuthenticator {
			get {
				return _MobileAuthenticator;
			}

			set {
				if (_MobileAuthenticator == value) {
					return;
				}

				_MobileAuthenticator = value;
				Save();
			}
		}

		private readonly object FileLock = new object();

		private string FilePath;

		internal static BotDatabase Load(string filePath) {
			if (string.IsNullOrEmpty(filePath)) {
				ASF.ArchiLogger.LogNullError(nameof(filePath));
				return null;
			}

			if (!File.Exists(filePath)) {
				return new BotDatabase(filePath);
			}

			BotDatabase botDatabase;

			try {
				botDatabase = JsonConvert.DeserializeObject<BotDatabase>(File.ReadAllText(filePath));
			} catch (Exception e) {
				ASF.ArchiLogger.LogGenericException(e);
				return null;
			}

			if (botDatabase == null) {
				ASF.ArchiLogger.LogNullError(nameof(botDatabase));
				return null;
			}

			botDatabase.FilePath = filePath;
			return botDatabase;
		}

		// This constructor is used when creating new database
		private BotDatabase(string filePath) {
			if (string.IsNullOrEmpty(filePath)) {
				throw new ArgumentNullException(nameof(filePath));
			}

			FilePath = filePath;
			Save();
		}

		// This constructor is used only by deserializer
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		private BotDatabase() { }

		internal void Save() {
			string json = JsonConvert.SerializeObject(this);
			if (string.IsNullOrEmpty(json)) {
				ASF.ArchiLogger.LogNullError(nameof(json));
				return;
			}

			lock (FileLock) {
				for (byte i = 0; i < 5; i++) {
					try {
						File.WriteAllText(FilePath, json);
						break;
					} catch (Exception e) {
						ASF.ArchiLogger.LogGenericException(e);
					}

					Thread.Sleep(1000);
				}
			}
		}
	}
}
