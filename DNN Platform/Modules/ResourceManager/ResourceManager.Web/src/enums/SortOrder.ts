import state from "../store/store"
 
 export class SortOrder{
     readonly ascending: SortOrderInfo;
     readonly descending: SortOrderInfo;
 
     constructor(){
         this.ascending = new SortOrderInfo("Ascending");
         this.descending = new SortOrderInfo("Descending");
     }
 }
 
 export class SortOrderInfo{
     readonly sortOrderKey: string;
 
     get localizedName(){
         return state.localization[`SortOrder_${this.sortOrderKey}`];
     }
 
     constructor(sortOrderKey: string = "Ascending"){
         this.sortOrderKey = sortOrderKey;
     }
 }
 
 export const sortOrder = new SortOrder();