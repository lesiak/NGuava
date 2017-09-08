using System;
using System.Collections;
using System.Collections.Generic;

namespace NGuava.Base
{
    public abstract class AbstractIterator<T> : IEnumerator<T> where T: class
    {
        private enum State {
            READY, NOT_READY, DONE, FAILED,
        }
        
        private State state = State.NOT_READY;
        
        protected AbstractIterator() {}
        
        private T nextVal;

        protected abstract T computeNext();

        protected T endOfData() {
            state = State.DONE;
            return null;
        }

        public bool MoveNext()
        {
            return hasNext();
        }

        public T Current => next();

        object IEnumerator.Current => next();

        public bool hasNext() {
            Preconditions.CheckState(state != State.FAILED);
            switch (state) {
                case State.DONE:
                    return false;
                case State.READY:
                    return true;
                
            }
            return tryToComputeNext();
        }

        private bool tryToComputeNext() {
            state = State.FAILED; // temporary pessimism
            nextVal = computeNext();
            if (state != State.DONE) {
                state = State.READY;
                return true;
            }
            return false;
        }

       
        public T next() {
            if (!hasNext()) {
                throw new InvalidOperationException();
            }
            state = State.NOT_READY;
            T result = nextVal;
            nextVal = null;
            return result;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}