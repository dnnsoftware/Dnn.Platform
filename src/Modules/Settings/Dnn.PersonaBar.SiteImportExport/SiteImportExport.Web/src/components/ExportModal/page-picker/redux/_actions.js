const ACT = require('./_action-types')

export const INIT = (tabs) => ( { type:ACT.INIT, tabs:tabs} )
export const DROP_DOWN = ({TabIdName, Tab}) =>  ({type:ACT.DROP_DOWN, TabIdName, Tab})
