import {createStore} from 'redux'
import deepFreeze from 'deep-freeze'
import * as ACT from './_action-types'


const InitialState = []
const pagepicker = (state=InitialState, {type, tabs, TabIdName, Tab}) => {


  deepFreeze(state)

  switch(type){
    case ACT.INIT:
      state = tabs
      return state

    case ACT.DROP_DOWN:
      const index = state
      .map((tab, i)=>{
        if(Tab.TabIdName==TabIdName){
          return i
        }
      }).filter(v=>!!v)[0]


      const split1 = state.slice(0,index)
      const split2 = state.slice(index+1)

      const new_state =  [...split1, Tab, ...split2]
      console.log(new_state)
      return new_state

    default:
    return state
  }


}


export const PagePickerStore = createStore(pagepicker)
