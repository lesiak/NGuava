using System;

namespace NGuava.Base
{
    public abstract class AbstractIterator<T> where T: class
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
    }
}