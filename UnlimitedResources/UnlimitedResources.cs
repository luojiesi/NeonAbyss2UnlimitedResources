using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine;
using UnityEngine.InputSystem;
using NEON.Game.Managers;
using NEON.Framework;
using NEON.Game.PowerUps.Skills;

namespace UnlimitedResources
{
    [BepInPlugin("com.neonabyss2.unlimitedresources", "Unlimited Resources", "1.0.0")]
    public class UnlimitedResourcesPlugin : BasePlugin
    {
        public override void Load()
        {
            AddComponent<UnlimitedResourcesBehaviour>();
            Log.LogInfo("Unlimited Resources mod loaded! Press F9 to toggle.");
        }
    }

    public class UnlimitedResourcesBehaviour : MonoBehaviour
    {
        private bool _enabled = true;

        private NEONPlayerState _playerState;
        private GameState _gameState;
        private string _lastScene;

        private const string KEY_BOMB = "DefaultBomb";
        private const string KEY_MAX_BOMB = "MaxBomb";
        private const string KEY_KEY = "Key";
        private const string KEY_MAX_KEY = "MaxKey";
        private const int TARGET_BOMBS = 99;
        private const int TARGET_KEYS = 99;
        private const int TARGET_FATE = 999;
        private const float REFILL_INTERVAL = 2f;
        private float _lastRefillTime;
        private int _lastCrystalCount = -1;

        void Update()
        {
            // Toggle with F9
            try
            {
                var keyboard = Keyboard.current;
                if (keyboard != null && keyboard.f9Key.wasPressedThisFrame)
                {
                    _enabled = !_enabled;
                    Debug.Log($"[UnlimitedResources] {(_enabled ? "ON" : "OFF")}");
                }
            }
            catch { }

            if (!_enabled) return;
            if (Time.time - _lastRefillTime < REFILL_INTERVAL) return;
            _lastRefillTime = Time.time;

            // Reset cached references on scene change
            try
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                if (scene != _lastScene)
                {
                    _lastScene = scene;
                    _playerState = null;
                    _gameState = null;
                    _lastCrystalCount = -1;
                }
            }
            catch { }

            // Find game objects (retries each tick until found)
            try
            {
                if (_playerState == null)
                    _playerState = Object.FindObjectOfType<NEONPlayerState>();
                if (_gameState == null)
                    _gameState = Object.FindObjectOfType<GameState>();
            }
            catch { }

            if (_playerState == null) return;

            // Bombs & Keys (via Attri system)
            try
            {
                var attrs = _playerState.attrs;
                if (attrs != null)
                {
                    if (!attrs.Has(KEY_MAX_BOMB) || attrs.Get(KEY_MAX_BOMB) < TARGET_BOMBS)
                        attrs.Set(KEY_MAX_BOMB, (double)TARGET_BOMBS, false);
                    if (!attrs.Has(KEY_BOMB) || attrs.Get(KEY_BOMB) < TARGET_BOMBS)
                        attrs.Set(KEY_BOMB, (double)TARGET_BOMBS, false);
                    if (!attrs.Has(KEY_MAX_KEY) || attrs.Get(KEY_MAX_KEY) < TARGET_KEYS)
                        attrs.Set(KEY_MAX_KEY, (double)TARGET_KEYS, false);
                    if (!attrs.Has(KEY_KEY) || attrs.Get(KEY_KEY) < TARGET_KEYS)
                        attrs.Set(KEY_KEY, (double)TARGET_KEYS, false);
                }
            }
            catch
            {
                _playerState = null;
            }

            // Crystals (via CostType resource system)
            // Instead of tracking max ourselves (which misses mid-tick increases),
            // just always add a large number and let the game cap at the real max.
            try
            {
                int crystalCount = _playerState.GetAmount(CostType.Crystal, false);
                if (_lastCrystalCount < 0)
                    _lastCrystalCount = crystalCount;

                if (crystalCount < _lastCrystalCount)
                    _playerState.AddResource(CostType.Crystal, _lastCrystalCount - crystalCount, true);

                // Always update to current (handles max increases from pickups)
                int updated = _playerState.GetAmount(CostType.Crystal, false);
                if (updated > _lastCrystalCount)
                    _lastCrystalCount = updated;
            }
            catch { }

            // Fate points (via SaveData)
            try
            {
                if (_gameState != null)
                {
                    var saveData = _gameState.CurrentSave;
                    if (saveData?.fatePointData != null && saveData.fatePointData.defaultPoint < TARGET_FATE)
                        saveData.fatePointData.defaultPoint = TARGET_FATE;
                }
            }
            catch
            {
                _gameState = null;
            }
        }
    }
}
