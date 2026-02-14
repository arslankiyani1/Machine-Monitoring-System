namespace MMS.Application.Common.Functional.Either
{
    public abstract class Either<TLeft, TRight>
    {
        public abstract bool IsLeft { get; }
        public bool IsRight => !IsLeft;

        public static Either<TLeft, TRight> FromLeft(TLeft left) => new Left(left);
        public static Either<TLeft, TRight> FromRight(TRight right) => new Right(right);

        public static implicit operator Either<TLeft, TRight>(TLeft left) => new Left(left);
        public static implicit operator Either<TLeft, TRight>(TRight right) => new Right(right);

        public abstract TResult Match<TResult>(
            Func<TRight, TResult> rightFunc,
            Func<TLeft, TResult> leftFunc);

        public abstract Task<TResult> MatchAsync<TResult>(
            Func<TRight, Task<TResult>> rightFunc,
            Func<TLeft, Task<TResult>> leftFunc);

        public TLeft? IfLeft() => this is Left left ? left.Value : default;
        public TRight? IfRight() => this is Right right ? right.Value : default;

        // Nested types to avoid name collisions
        private sealed class Left : Either<TLeft, TRight>
        {
            public TLeft Value { get; }

            public Left(TLeft value) => Value = value;

            public override bool IsLeft => true;

            public override TResult Match<TResult>(Func<TRight, TResult> rightFunc, Func<TLeft, TResult> leftFunc)
                => leftFunc(Value);

            public override Task<TResult> MatchAsync<TResult>(
                Func<TRight, Task<TResult>> rightFunc,
                Func<TLeft, Task<TResult>> leftFunc)
                => leftFunc(Value);
        }

        private sealed class Right : Either<TLeft, TRight>
        {
            public TRight Value { get; }

            public Right(TRight value) => Value = value;

            public override bool IsLeft => false;

            public override TResult Match<TResult>(Func<TRight, TResult> rightFunc, Func<TLeft, TResult> leftFunc)
                => rightFunc(Value);

            public override Task<TResult> MatchAsync<TResult>(
                Func<TRight, Task<TResult>> rightFunc,
                Func<TLeft, Task<TResult>> leftFunc)
                => rightFunc(Value);
        }
    }
}
