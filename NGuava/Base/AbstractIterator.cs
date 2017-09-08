using System;
using System.Collections;
using System.Collections.Generic;

namespace NGuava.Base
{
    public abstract class AbstractIterator<T> : IEnumerator<T> where T: class
    {
        private enum State {
            /** We have computed the next element. */
            READY,

            /** We haven't yet computed any element. */
            NOT_READY,


            /** We have reached the end of the data and are finished. */
            DONE,

            /** We've suffered an exception and are kaput. */
            FAILED,
        }
        
        private State state = State.NOT_READY;
        
        private T nextVal;

        /// <summary>
        /// 
        /// Returns the next element. <b>Note:</b> the implementation must call 
        /// {@link #EndOfData()} when there are no elements left in the iteration. Failure to
        /// do so could result in an infinite loop.
        /// </summary>
        /// <returns></returns>
        protected abstract T ComputeNext();

        protected T EndOfData() {
            state = State.DONE;
            return null;
        }

        public bool MoveNext()
        {
            Preconditions.CheckState(state != State.FAILED);
            if (state == State.DONE)
            {
                return false;
            }
            return TryToComputeNext();
        }


        private bool TryToComputeNext()
        {
            state = State.FAILED; // temporary pessimism
            nextVal = ComputeNext();
            if (state != State.DONE)
            {
                state = State.READY;
                return true;
            }
            return false;
        }

        public T Current
        {
            get {
                Preconditions.CheckState(state != State.FAILED);
                if (state == State.DONE || state == State.NOT_READY)
                {
                    throw new InvalidOperationException();
                }
              // state = State.NOT_READY;
              // T result = nextVal;
              //  nextVal = null;
                return nextVal;
            }
        }

        object IEnumerator.Current => Current;

        

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            
        }
    }
}