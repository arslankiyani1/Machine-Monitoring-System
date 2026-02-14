namespace MMS.Application.Common.Functional.Either
{
    public class Left<TLeft, TRight> : Either<TLeft, TRight>
    {
        public TLeft Content { get; }

        public override bool IsLeft => true;

        public Left(TLeft content) => Content = content;

        public static implicit operator TLeft(Left<TLeft, TRight> obj) => obj.Content;

        public override TResult Match<TResult>(
            Func<TRight, TResult> rightFunc,
            Func<TLeft, TResult> leftFunc)
        {
            if (leftFunc == null) throw new ArgumentNullException(nameof(leftFunc));
            return leftFunc(Content);
        }

        public override Task<TResult> MatchAsync<TResult>(
            Func<TRight, Task<TResult>> rightFunc,
            Func<TLeft, Task<TResult>> leftFunc)
        {
            if (leftFunc == null) throw new ArgumentNullException(nameof(leftFunc));
            return leftFunc(Content);
        }
    }
}
