namespace ScheduleR.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using dddlib;

    public sealed class Callback : ValueObject<Callback>
    {
        public Callback(string id, Uri url)
        {
            Guard.Against.Null(() => id);
            Guard.Against.Null(() => url);

            if (!id.Trim().All(char.IsLetterOrDigit))
            {
                throw new BusinessException("The callback ID must only contain alpha-numeric characters.");
            }

            if (id.Trim().Length > 50)
            {
                throw new BusinessException("The callback ID must be less than 50 characters in length.");
            }

            this.Id = id.Trim();
            this.Url = url;
        }

        public string Id { get; private set; }

        public Uri Url { get; private set; }

        internal class Comparer : IEqualityComparer<Callback>
        {
            public bool Equals(Callback x, Callback y)
            {
                // LINK (Cameron): http://blog.iandavis.com/2002/04/problems-with-the-net-uri-implementation/
                return string.Equals(x.Id, y.Id, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.Url.ToString(), y.Url.ToString(), StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(Callback obj)
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + StringComparer.OrdinalIgnoreCase.GetHashCode(obj);
                    hash = hash * 23 + StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Url.ToString());
                    return hash;
                }
            }
        }
    }
}
