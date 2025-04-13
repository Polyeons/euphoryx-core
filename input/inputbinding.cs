using UnityEngine;

namespace tech.polyeons.euphoryx.input
{
    public struct InputBinding
    {
        public KeyCode KeyboardKey { get; set; }
        public string GamepadAxis { get; set; }
        public GamepadButton GamepadButton { get; set; }
    }

    public enum GamepadButton
    {
        None,
        A,
        B,
        X,
        Y,
        LB,
        RB,
        LT,
        RT,
        Start,
        Select
    }

    public struct InputState
    {
        public float Value { get; set; }
        public bool IsPressed { get; set; }
    }
}