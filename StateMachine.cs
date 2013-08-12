using System;
using System.Collections;
using System.Collections.Generic;

namespace AssemblyCSharp
{
	public interface IState
	{
		/// <summary>
		/// Called when entering state.
		/// </summary>
		void OnEnter();
		
		/// <summary>
		/// Called when leaving state
		/// </summary>
		void OnExit(IState nextState);
		
		/// <summary>
		/// Called when this state is pushed to the stack. Called before OnExit.
		/// </summary>
		void OnPush();
		
		/// <summary>
		/// Called when this state is "popped" aka restored from the stack. Before OnEnter.
		/// </summary>
		void OnPop();
	}
	
	public class StateMachine<ConcreteState> where ConcreteState : IState
	{
		Stack<ConcreteState> stack;
		
		public ConcreteState current;
		
		public StateMachine (ConcreteState initialState)
		{
			this.stack = new Stack<ConcreteState>();	
			current = initialState;
		}
				
		public void PushState(ConcreteState state)
		{
			current.OnPush();
			stack.Push(current);
			ChangeState(state);
		}
		
		public void PopState()
		{
			ConcreteState state = stack.Pop();
			state.OnPop();
			ChangeState(state);
		}
		
		public void ChangeState(ConcreteState state)
		{
			current.OnExit(state);
			current = state;
			current.OnEnter();
		}
		
	}
}

