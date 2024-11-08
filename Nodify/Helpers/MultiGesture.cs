using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Nodify
{
    public class MultiInputEventArgs : InputEventArgs
    {
        public InputEventArgs[] Events { get; private set; }
        public MultiInputEventArgs(params InputEventArgs[] events)
            : base(events[0].Device, events[0].Timestamp)
        {
            Events = events;
        }
    }

    public class MouseWheelGesture : InputGesture
    {
        public ModifierKeys Modifiers { get; set; }

        public MouseWheelGesture() { }
        public MouseWheelGesture(ModifierKeys modifiers) => Modifiers = modifiers;

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            return inputEventArgs is MouseWheelEventArgs && Keyboard.Modifiers.HasFlag(Modifiers);
        }
    }

    /// <summary>Combines multiple input gestures.</summary>
    public class MultiGesture : InputGesture
    {
        public static readonly MultiGesture None = new MultiGesture(Match.Any);

        /// <summary>The strategy used by <see cref="Matches(object, InputEventArgs)"/>.</summary>
        public enum Match
        {
            /// <summary>At least one gesture must _match.</summary>
            Any,
            /// <summary>All gestures must _match.</summary>
            All
        }

        private readonly InputGesture[] _gestures;
        private readonly Match _match;

        /// <summary>Constructs an instance of a <see cref="MultiGesture"/>.</summary>
        /// <param name="match">The matching strategy.</param>
        /// <param name="gestures">The input gestures.</param>
        public MultiGesture(Match match, params InputGesture[] gestures)
        {
            _gestures = gestures;
            _match = match;
        }

        /// <inheritdoc />
        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (_match == Match.Any)
            {
                return MatchesAny(targetElement, inputEventArgs);
            }

            return MatchesAll(targetElement, inputEventArgs);
        }

        private bool MatchesAll(object targetElement, InputEventArgs inputEventArgs)
        {
            for (int i = 0; i < _gestures.Length; i++)
            {
                if (!Matches(_gestures[i], targetElement, inputEventArgs))
                {
                    return false;
                }
            }
            return true;
        }

        private bool MatchesAny(object targetElement, InputEventArgs inputEventArgs)
        {
            for (int i = 0; i < _gestures.Length; i++)
            {
                if (Matches(_gestures[i], targetElement, inputEventArgs))
                {
                    return true;
                }
            }
            return false;
        }

        private bool Matches(InputGesture gesture, object targetElement, InputEventArgs inputEventArgs)
        {
            if (inputEventArgs is MultiInputEventArgs multiEvents && !(gesture is MultiGesture))
                return multiEvents.Events.Any(e => gesture.Matches(targetElement, e));
            return gesture.Matches(targetElement, inputEventArgs);
        }
    }

    /// <inheritdoc cref="MultiGesture.Match.Any" />
    public sealed class AnyGesture : MultiGesture
    {
        public AnyGesture(params InputGesture[] gestures) : base(Match.Any, gestures)
        {
        }
    }

    /// <inheritdoc cref="MultiGesture.Match.All" />
    public sealed class AllGestures : MultiGesture
    {
        public AllGestures(params InputGesture[] gestures) : base(Match.All, gestures)
        {
        }
    }

    /// <summary>
    /// An input gesture that allows changing its logic at runtime without changing its reference.
    /// Useful for classes that capture the object reference without the posibility of updating it. (e.g. <see cref="EditorCommands"/>)
    /// </summary>
    public sealed class InputGestureRef : InputGesture
    {
        /// <summary>The referenced gesture.</summary>
        public InputGesture Value { get; set; } = MultiGesture.None;

        private InputGestureRef() { }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            return Value.Matches(targetElement, inputEventArgs);
        }

        public static implicit operator InputGestureRef(MouseGesture gesture)
            => new InputGestureRef { Value = gesture };

        public static implicit operator InputGestureRef(MouseWheelGesture gesture)
            => new InputGestureRef { Value = gesture };

        public static implicit operator InputGestureRef(KeyGesture gesture)
            => new InputGestureRef { Value = gesture };

        public static implicit operator InputGestureRef(MultiGesture gesture)
            => new InputGestureRef { Value = gesture };
    }
}
