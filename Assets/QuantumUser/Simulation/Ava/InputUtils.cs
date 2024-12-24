namespace Quantum.Ava
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    public enum Direction
    {
        Forward = 0,
        Backward = 1
    }
    
    [Preserve]
    public unsafe class InputUtils
    {
        public static Input[] GetLastInputs(FighterData* fd, int count)
        {
            Input[] lastInputs = new Input[count];
            for (int i = 0; i < count; i++)
            {
                int index = (fd->InputHeadIndex - i + fd->InputHistory.Length) % fd->InputHistory.Length;
                lastInputs[i] = fd->InputHistory[index];
            }
            return lastInputs;
        }
        
        public static bool CheckDash(FighterData* fd, int dashAllowFrames, Direction direction)
        {
            var inputs = GetLastInputs(fd, dashAllowFrames + 5);
            var oppositeDir = direction == Direction.Forward ? Direction.Backward : Direction.Forward;

            IsDirection(fd, inputs[0], direction, out var button);
            if (!button.WasPressed)
                return false;

            for (int i = 1; i < dashAllowFrames; i++)
            {
                if (IsDirection(fd, inputs[i], oppositeDir))
                    return false;
                
                if (IsDirection(fd, inputs[i], direction))
                {
                    for (int j = i + 1; j < i + 5; j++)
                    {
                        if (!IsDirection(fd, inputs[j], direction) && !IsDirection(fd, inputs[j], oppositeDir))
                            return true;
                    }

                    return false;
                }
            }

            return false;
        }

        public static bool IsDirection(FighterData* fd, Input input, Direction direction)
        {
            switch (direction)
            {
                case Direction.Forward:
                    return (input.Right && fd->IsFacingRight) || (input.Left && !fd->IsFacingRight);
                    
                case Direction.Backward:
                    return (input.Left && fd->IsFacingRight) || (input.Right && !fd->IsFacingRight);
            }

            return false;
        }
        
        public static bool IsDirection(FighterData* fd, Input input, Direction dir, out Button button)
        {
            button = default;
            
            switch (dir)
            {
                case Direction.Forward:
                    if (input.Right && fd->IsFacingRight)
                    {
                        button = input.Right;
                        return true;
                    }

                    if (input.Left && !fd->IsFacingRight)
                    {
                        button = input.Left;
                        return true;
                    }

                    return false;
                    
                case Direction.Backward:
                    if (input.Left && fd->IsFacingRight)
                    {
                        button = input.Left;
                        return true;
                    }

                    if (input.Right && !fd->IsFacingRight)
                    {
                        button = input.Right;
                        return true;
                    }

                    return false;
            }

            return false;
        }

        public static StateID CheckJumpType(FighterData* fd, Input input)
        {
            if (IsDirection(fd, input, Direction.Forward)) return StateID.JUMP_FORWARD;
            if (IsDirection(fd, input, Direction.Backward)) return StateID.JUMP_BACKWARD;
            return StateID.JUMP_NEUTRAL;
        }
    }
}
