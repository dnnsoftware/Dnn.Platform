import React from "react";
import { mount, render, shallow } from "enzyme";
import Breadcrumbs from "./Breadcrumbs";

describe("Breadcrumbs",()=>{
    let onSelectedItemReturn = -1;

    const onSelectedItem = (item) => {
        console.log(item);
        onSelectedItemReturn = item;
    }

    const item = {
        id:1,
        name:"Item1"
    };

    const items = [
        {
            id:1,
            name:"Item1"
        },{
            id:2,
            name:"Item2"
        }];

    test("Breadcrumbs renders",()=>{
        let breadcrumbs = mount(<Breadcrumbs items={[]} onSelectedItem={onSelectedItem}/>);
        expect(breadcrumbs.find(".breadcrumbs-container").length).toBe(1);
    });

    test("Breadcrumbs renders empty when no itens passed", ()=> {
        let breadcrumbs = mount(<Breadcrumbs items={[]} onSelectedItem={onSelectedItem}/>);
        expect(breadcrumbs.find(".breadcrumbs-container").children().length).toBe(0); 
    });

    test("Breadcrumb render one item", ()=> {
        let breadcrumbs = mount(<Breadcrumbs items={[item]} onSelectedItem={onSelectedItem}/>);
        expect(breadcrumbs.find("span").text()).toBe("Item1");
    });

    test("Breadcrumb render two items", ()=> {
        let breadcrumbs = mount(<Breadcrumbs items={items} onSelectedItem={onSelectedItem}/>);
        expect(breadcrumbs.find("span").at(1).text()).toBe("Item2");
    });

    test("Breadcrumb click", () => {
        let breadcrumbs = shallow(<Breadcrumbs items={items} onSelectedItem={()=>console.log("this is sparta!!!")}/>);
        // console.log(breadcrumbs.debug());
        console.log(breadcrumbs.find(".breadcrumbs-container").childAt(0).debug());
        breadcrumbs.find(".breadcrumbs-container").childAt(0).simulate("click");
        // let wrapper = breadcrumbs.find(".breadcrumbs-container").childAt(0);
        // wrapper.simulate("click");

        // breadcrumbs.find("span").at(0).simulate("click");

        // expect(onSelectedItemReturn).toBe(items[0].id);
    });

    test("Add more then max items to breadcrumb", () =>{


    })

});