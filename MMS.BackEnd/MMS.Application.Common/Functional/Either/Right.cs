namespace MMS.Application.Common.Functional.Either
{
    public class Right<TLeft, TRight> : Either<TLeft, TRight>
    {
        public TRight Content { get; }

        public Right(TRight content)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public override bool IsLeft => false;

        public static implicit operator TRight(Right<TLeft, TRight> obj) => obj.Content;

        public override TResult Match<TResult>(
            Func<TRight, TResult> rightFunc,
            Func<TLeft, TResult> leftFunc)
        {
            if (rightFunc == null) throw new ArgumentNullException(nameof(rightFunc));
            if (leftFunc == null) throw new ArgumentNullException(nameof(leftFunc));
            return rightFunc(Content);
        }

        public override Task<TResult> MatchAsync<TResult>(
            Func<TRight, Task<TResult>> rightFunc,
            Func<TLeft, Task<TResult>> leftFunc)
        {
            if (rightFunc == null) throw new ArgumentNullException(nameof(rightFunc));
            if (leftFunc == null) throw new ArgumentNullException(nameof(leftFunc));
            return rightFunc(Content);
        }
    }
}
