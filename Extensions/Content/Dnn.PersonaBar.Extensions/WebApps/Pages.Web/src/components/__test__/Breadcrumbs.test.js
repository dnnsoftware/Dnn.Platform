import React from "react";
import { shallow } from "enzyme";
import Breadcrumbs from "../Breadcrumbs";

describe("Breadcrumbs",()=>{
    let onSelectedItemReturn = -1;

    //Callback on click Breadcrumbs
    const onSelectedItem = (item) => {
        onSelectedItemReturn = item;
    };

    const item = {
        id:1,
        name:"Item1",
        tabId:1
    };

    const items = [
        {
            id:1,
            name:"Item1",
            tabId:1
        },{
            id:2,
            name:"Item2",
            tabId:2
        }];

    const itemsMax = [
        ...items,
        {
            id:3,
            name:"Item3",
            tabId:3   
        },{
            id:4,
            name:"Item4" ,
            tabId:4
        },{
            id:5,
            name:"Item5",
            tabId:5
        },{
            id:6,
            name:"Item6",
            tabId:6
        }];
       

    it("Breadcrumbs renders",()=>{
        let breadcrumbs = shallow(<Breadcrumbs items={[]} onSelectedItem={onSelectedItem}/>);
        expect(breadcrumbs.find(".breadcrumbs-container").length).toBe(1);
    });

    it("Breadcrumbs renders empty when no itens passed", ()=> {
        let breadcrumbs = shallow(<Breadcrumbs items={[]} onSelectedItem={onSelectedItem}/>);
        expect(breadcrumbs.find(".breadcrumbs-container").children().length).toBe(0); 
    });

    it("Breadcrumb render one item", ()=> {
        let breadcrumbs = shallow(<Breadcrumbs items={[item]} onSelectedItem={onSelectedItem}/>);
        expect(breadcrumbs.find("span").text()).toBe("Item1");
    });

    it("Breadcrumb render two items", ()=> {
        let breadcrumbs = shallow(<Breadcrumbs items={items} onSelectedItem={onSelectedItem}/>);
        expect(breadcrumbs.find("span").at(1).text()).toBe("Item2");
    });

    it("Breadcrumb click", () => {
        let breadcrumbs = shallow(<Breadcrumbs items={items} onSelectedItem={onSelectedItem}/>);
        breadcrumbs.find(".breadcrumbs-container").childAt(1).simulate("click");
        expect(onSelectedItemReturn).toBe(items[1].tabId);
    });

    it("Add six items to breadccrumb", () =>{
        let breadcrumbs = shallow(<Breadcrumbs items={itemsMax} onSelectedItem={onSelectedItem}/>);

        const countChildren = breadcrumbs.find(".breadcrumbs-container").children().length; 
        expect(countChildren).toBe(5);
    });

    it('Click on start point breadcrumb',()=>{
        let onSelectedItemCallback = jest.fn();
        let breadcrumbs = shallow(<Breadcrumbs items={itemsMax} onSelectedItem={onSelectedItem}/>);

        breadcrumbs.find(".more").simulate("click");
        expect(onSelectedItemCallback).toHaveBeenCalledTimes(0);
    });
});