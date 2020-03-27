namespace Straitjacket.Subnautica.Mods.AutoLoad.SMLHelper.V2.Utility
{
    using UnityEngine;

    internal static class KeyCodeUtils
    {
        internal static GameInput.InputState GetInputStateForKeyCode(KeyCode keyCode)
        {
            var inputState = default(GameInput.InputState);
            if (!GameInput.clearInput && !GameInput.scanningInput)
            {
                for (var i = 0; i < GameInput.inputs.Count; i++)
                {
                    if (GameInput.inputs[i].keyCode == keyCode)
                    {
                        inputState.flags |= GameInput.inputStates[i].flags;
                        inputState.timeDown = Mathf.Max(
                            inputState.timeDown, GameInput.inputStates[i].timeDown
                        );
                        break;
                    }
                }
            }
            return inputState;
        }

        /// <summary>
        /// Check this is the first frame a key has been pressed
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns>True during the first frame a key has been pressed, otherwise false</returns>
        /// <seealso cref="KeyCode"/>
        /// <seealso cref="GetKeyDown(string)"/>
        public static bool GetKeyDown(KeyCode keyCode)
            => (GetInputStateForKeyCode(keyCode).flags & GameInput.InputStateFlags.Down) > 0U;
        /// <summary>
        /// Check this is the first frame a key has been pressed.
        /// </summary>
        /// <param name="s"></param>
        /// <returns>True during the first frame a key has been pressed, otherwise false</returns>
        /// <seealso cref="GetKeyDown(KeyCode)"/>
        public static bool GetKeyDown(string s) 
            => GetKeyDown(global::SMLHelper.V2.Utility.KeyCodeUtils.StringToKeyCode(s));

        /// <summary>
        /// Check a key is currently held down
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns>True every frame a key is held down, otherwise false</returns>
        /// <seealso cref="KeyCode"/>
        /// <seealso cref="GetKeyHeld(string)"/>
        public static bool GetKeyHeld(KeyCode keyCode)
            => (GetInputStateForKeyCode(keyCode).flags & GameInput.InputStateFlags.Held) > 0U;
        /// <summary>
        /// Check a key is currently held down
        /// </summary>
        /// <param name="s"></param>
        /// <returns>True every frame a key is held down, otherwise false</returns>
        /// <seealso cref="GetKeyHeld(KeyCode)"/>
        public static bool GetKeyHeld(string s) 
            => GetKeyHeld(global::SMLHelper.V2.Utility.KeyCodeUtils.StringToKeyCode(s));

        /// <summary>
        /// Check how long a key has been held down
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        /// <seealso cref="KeyCode"/>
        /// <seealso cref="GetKeyHeldTime(string)"/>
        public static float GetKeyHeldTime(KeyCode keyCode)
        {
            var inputStateForKeyCode = GetInputStateForKeyCode(keyCode);
            if ((inputStateForKeyCode.flags & GameInput.InputStateFlags.Held) == 0U)
            {
                return 0f;
            }
            return Time.unscaledTime - inputStateForKeyCode.timeDown;
        }
        /// <summary>
        /// Check how long a key has been held down
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <seealso cref="GetKeyHeldTime(KeyCode)"/>
        public static float GetKeyHeldTime(string s) 
            => GetKeyHeldTime(global::SMLHelper.V2.Utility.KeyCodeUtils.StringToKeyCode(s));

        /// <summary>
        /// Check this is the frame a key has been released
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns>True during the first frame a key has been released, otherwise false</returns>
        /// <seealso cref="KeyCode"/>
        /// <seealso cref="GetKeyUp(string)"/>
        public static bool GetKeyUp(KeyCode keyCode)
            => (GetInputStateForKeyCode(keyCode).flags & GameInput.InputStateFlags.Up) > 0U;
        /// <summary>
        /// Check this is the first frame a key has been released
        /// </summary>
        /// <param name="s"></param>
        /// <returns>True during the first frame a key has been released, otherwise false</returns>
        /// <seealso cref="GetKeyUp(KeyCode)"/>
        public static bool GetKeyUp(string s) 
            => GetKeyUp(global::SMLHelper.V2.Utility.KeyCodeUtils.StringToKeyCode(s));

        /// <summary>
        /// Gets the analog value for a <seealso cref="KeyCode"/> following the same logic as
        /// <seealso cref="GameInput.GetAnalogValueForButton(GameInput.Button)"/>
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns>1f while a key is being held, otherwise 0f</returns>
        /// <seealso cref="KeyCode"/>
        /// <seealso cref="GetAnalogValueForKey(string)"/>
        public static float GetAnalogValueForKey(KeyCode keyCode)
        {
            var inputStateForKeyCode = GetInputStateForKeyCode(keyCode);
            if ((inputStateForKeyCode.flags & GameInput.InputStateFlags.Held) != 0U)
            {
                return 1f;
            }
            return 0f;
        }
        /// <summary>
        /// Gets the analog value for a key by <seealso cref="string"/> value, following the same logic as
        /// <seealso cref="GameInput.GetAnalogValueForButton(GameInput.Button)"/>
        /// </summary>
        /// <param name="s"></param>
        /// <returns>1f while a key is being held, otherwise 0f</returns>
        /// <seealso cref="GetAnalogValueForKey(KeyCode)"/>
        public static float GetAnalogValueForKey(string s) 
            => GetAnalogValueForKey(global::SMLHelper.V2.Utility.KeyCodeUtils.StringToKeyCode(s));
    }
}
