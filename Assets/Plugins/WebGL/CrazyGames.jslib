mergeInto(LibraryManager.library, {
  CG_InitSdk: function (gameObjectNamePtr) {
    var gameObjectName = UTF8ToString(gameObjectNamePtr);

    if (!window.CrazyGamesUnityBridge) {
      window.CrazyGamesUnityBridge = {
        sdkReady: false,
        sdkInitPromise: null,
        scriptPromise: null,

        sendMessage: function (target, method, value) {
          try {
            if (typeof value === "undefined") {
              SendMessage(target, method);
            } else {
              SendMessage(target, method, value);
            }
          } catch (error) {
            console.error("CrazyGames bridge SendMessage error", error);
          }
        },

        ensureScriptLoaded: function () {
          if (window.CrazyGames && window.CrazyGames.SDK) {
            return Promise.resolve();
          }

          if (this.scriptPromise) {
            return this.scriptPromise;
          }

          this.scriptPromise = new Promise(function (resolve, reject) {
            var existing = document.querySelector('script[data-crazygames-sdk="true"]');
            if (existing) {
              if (window.CrazyGames && window.CrazyGames.SDK) {
                resolve();
                return;
              }

              if (existing.dataset.loaded === "true") {
                resolve();
                return;
              }

              existing.addEventListener("load", function () { resolve(); }, { once: true });
              existing.addEventListener("error", function () { reject(new Error("CrazyGames SDK script load failed")); }, { once: true });
              return;
            }

            var script = document.createElement("script");
            script.src = "https://sdk.crazygames.com/crazygames-sdk-v3.js";
            script.async = true;
            script.dataset.crazygamesSdk = "true";
            script.onload = function () {
              script.dataset.loaded = "true";
              resolve();
            };
            script.onerror = function () { reject(new Error("CrazyGames SDK script load failed")); };
            document.head.appendChild(script);
          });

          return this.scriptPromise;
        },

        init: function (target) {
          var self = this;

          if (self.sdkReady) {
            self.sendMessage(target, "HandleSdkInitialized", "true");
            return;
          }

          if (self.sdkInitPromise) {
            self.sdkInitPromise
              .then(function () {
                self.sendMessage(target, "HandleSdkInitialized", "true");
              })
              .catch(function (error) {
                console.error("CrazyGames SDK init failed", error);
                self.sendMessage(target, "HandleSdkInitialized", "false");
              });
            return;
          }

          self.sdkInitPromise = self.ensureScriptLoaded()
            .then(function () {
              if (!window.CrazyGames || !window.CrazyGames.SDK) {
                throw new Error("CrazyGames SDK unavailable after script load");
              }

              return window.CrazyGames.SDK.init();
            })
            .then(function () {
              self.sdkReady = true;
              self.sendMessage(target, "HandleSdkInitialized", "true");
            })
            .catch(function (error) {
              console.error("CrazyGames SDK init failed", error);
              self.sendMessage(target, "HandleSdkInitialized", "false");
              throw error;
            });
        },

        requestAd: function (adType, target) {
          var self = this;

          var runRequest = function () {
            if (!window.CrazyGames || !window.CrazyGames.SDK || !window.CrazyGames.SDK.ad) {
              self.sendMessage(target, "HandleAdError", adType + "|sdkUnavailable");
              return;
            }

            try {
              window.CrazyGames.SDK.ad.requestAd(adType, {
                adStarted: function () {
                  self.sendMessage(target, "HandleAdStarted", adType);
                },
                adFinished: function () {
                  self.sendMessage(target, "HandleAdFinished", adType);
                },
                adError: function (error) {
                  var code = error && error.code ? error.code : "other";
                  self.sendMessage(target, "HandleAdError", adType + "|" + code);
                }
              });
            } catch (error) {
              console.error("CrazyGames requestAd failed", error);
              self.sendMessage(target, "HandleAdError", adType + "|exception");
            }
          };

          if (self.sdkReady) {
            runRequest();
            return;
          }

          self.init(target);
          self.sdkInitPromise
            .then(function () {
              runRequest();
            })
            .catch(function () {
              self.sendMessage(target, "HandleAdError", adType + "|initFailed");
            });
        },

        gameplayStart: function () {
          if (!this.sdkReady || !window.CrazyGames || !window.CrazyGames.SDK || !window.CrazyGames.SDK.game) {
            return;
          }

          try {
            window.CrazyGames.SDK.game.gameplayStart();
          } catch (error) {
            console.error("CrazyGames gameplayStart failed", error);
          }
        },

        gameplayStop: function () {
          if (!this.sdkReady || !window.CrazyGames || !window.CrazyGames.SDK || !window.CrazyGames.SDK.game) {
            return;
          }

          try {
            window.CrazyGames.SDK.game.gameplayStop();
          } catch (error) {
            console.error("CrazyGames gameplayStop failed", error);
          }
        }
      };
    }

    window.CrazyGamesUnityBridge.init(gameObjectName);
  },

  CG_RequestAd: function (adTypePtr, gameObjectNamePtr) {
    var adType = UTF8ToString(adTypePtr);
    var gameObjectName = UTF8ToString(gameObjectNamePtr);

    if (!window.CrazyGamesUnityBridge) {
      console.error("CrazyGames bridge not initialized before ad request");
      return;
    }

    window.CrazyGamesUnityBridge.requestAd(adType, gameObjectName);
  },

  CG_GameplayStart: function () {
    if (window.CrazyGamesUnityBridge) {
      window.CrazyGamesUnityBridge.gameplayStart();
    }
  },

  CG_GameplayStop: function () {
    if (window.CrazyGamesUnityBridge) {
      window.CrazyGamesUnityBridge.gameplayStop();
    }
  }
});
