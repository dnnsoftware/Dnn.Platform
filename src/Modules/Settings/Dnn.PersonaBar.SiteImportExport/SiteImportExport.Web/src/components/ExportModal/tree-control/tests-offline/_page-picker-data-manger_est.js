const Tabs = require('../mocks/_portal-tabs').tabs
const TabRootOnlySelected = require('../mocks/_portal-tabs-selected-root-only').tabs
const TabSubset = require('../mocks/_sub-set-portal-tabs')


const flattend_tab_data = require('./_expected.flattened-tabs')
const updated_child_tabs = require('./_expected.updated-child-tabs')

import {PagePickerDataManager} from '../helpers/_page-picker-data-manager'

const ppdm = new PagePickerDataManager()

test.skip('flatten() will return and internally store a flattend version of the PagePickerData', () => {
    ppdm.flatten(TabSubset)
    const result = ppdm.export()
    const expected = flattend_tab_data
    console.log(result)
    expect(result).toEqual(expected)
})

test.skip('replace() entire storage', ()=>{
    ppdm.replace({hello:'joe'})
    const result = ppdm.export()
    const expected = {hello:'joe'}
    console.log(result)
    expect(result).toEqual(expected)
})

test.skip('update() will update a specific tab object', ()=>{
    ppdm.replace(
      {'20-Home': {
        Name: 'Home',
        TabId: '20',
        ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
        Tooltip: 'Homepage of the site',
        ParentTabId: -1,
        HasChildren: true,
        IsOpen: false,
        Selectable: true,
        CheckedState: 0,
        ChildTabs: ['211-Inner Child']
    },
    '21-Our Products': {
        Name: 'Our Products',
        TabId: '21',
        ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Everyone.png',
        Tooltip: 'Page is visible to everyone',
        ParentTabId: -1,
        HasChildren: false,
        IsOpen: false,
        Selectable: true,
        CheckedState: 0,
        ChildTabs: []},
    })

    ppdm.update({id:'21-Our Products'}, {CheckedState:1})
    const result = ppdm.export()
    const expected = {'20-Home': {
            Name: 'Home',
            TabId: '20',
            ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
            Tooltip: 'Homepage of the site',
            ParentTabId: -1,
            HasChildren: true,
            IsOpen: false,
            Selectable: true,
            CheckedState: 0,
            ChildTabs: ['211-Inner Child']
        },
        '21-Our Products': {
            Name: 'Our Products',
            TabId: '21',
            ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Everyone.png',
            Tooltip: 'Page is visible to everyone',
            ParentTabId: -1,
            HasChildren: false,
            IsOpen: false,
            Selectable: true,
            CheckedState: 1,
            ChildTabs: []},
        }


      expect(result).toEqual(expected)
})

test.skip('append() will recursively flatten new tabs and add them to storage', ()=>{
  ppdm.replace({
  '21-Our Products': {
      Name: 'Our Products',
      TabId: '21',
      ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Everyone.png',
      Tooltip: 'Page is visible to everyone',
      ParentTabId: -1,
      HasChildren: false,
      IsOpen: false,
      Selectable: true,
      CheckedState: 0,
      ChildTabs: []},
  })

  ppdm.append({
    Name: 'Appended Child',
    TabId: '100',
    ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Everyone.png',
    Tooltip: 'Page is visible to everyone',
    ParentTabId: 21,
    HasChildren: true,
    IsOpen: false,
    Selectable: true,
    CheckedState: 0,
    ChildTabs: [
      {
        Name: 'Appended Inner Child',
        TabId: '101',
        ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Everyone.png',
        Tooltip: 'Page is visible to everyone',
        ParentTabId: 100,
        HasChildren: false,
        IsOpen: false,
        Selectable: true,
        CheckedState: 0,
        ChildTabs:[]
      }
    ]
  })


  const result = ppdm.export()
  const expected = {
    '21-Our Products': {
        Name: 'Our Products',
        TabId: '21',
        ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Everyone.png',
        Tooltip: 'Page is visible to everyone',
        ParentTabId: -1,
        HasChildren: false,
        IsOpen: false,
        Selectable: true,
        CheckedState: 0,
        ChildTabs: []
      },
    '100-Appended Child': {
            Name: 'Appended Child',
            TabId: '100',
            ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Everyone.png',
            Tooltip: 'Page is visible to everyone',
            ParentTabId: 21,
            HasChildren: true,
            IsOpen: false,
            Selectable: true,
            CheckedState: 0,
            ChildTabs: ['101-Appended Inner Child']
      },
    '101-Appended Inner Child' :{
        Name: 'Appended Inner Child',
        TabId: '101',
        ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Everyone.png',
        Tooltip: 'Page is visible to everyone',
        ParentTabId: 100,
        HasChildren: false,
        IsOpen: false,
        Selectable: true,
        CheckedState: 0,
        ChildTabs:[]
      }
    }


    expect(result).toEqual(expected)

})

test.skip('childrenOf() will return an array of childtabs for a parent tab', ()=>{
    ppdm.flatten(Tabs)
    const result = ppdm.childrenOf({id:'20-Home'})

    const expected = [
      {
      "Name": "Inner Child",
      "TabId": "211",
      "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Everyone.png",
      "Tooltip": "Page is visible to everyone",
      "ParentTabId": 20,
      "HasChildren": false,
      "IsOpen": false,
      "Selectable": true,
      "CheckedState": 0,
      "ChildTabs": []
    },
    {
      "Name": "Inner Child 2",
      "TabId": "212",
      "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Everyone.png",
      "Tooltip": "Page is visible to everyone",
      "ParentTabId": 20,
      "HasChildren": false,
      "IsOpen": false,
      "Selectable": true,
      "CheckedState": 0,
      "ChildTabs": []
    }
    ]

    expect(result).toEqual(expected)

})

test.skip('_updateParentChildTabs will update a parent tab ChildTabs with a new id ref', ()=> {
    ppdm.flatten(Tabs)
    ppdm._updateParentChildTabs()

    const result = ppdm.export()
    const expected =  updated_child_tabs
    console.log(result)


    expect(result).toEqual(expected)
})

test.skip('getBy() accepts a unique key that exists on a tab and returns that object', ()=>{
    ppdm.flatten(TabSubset)
    const result = ppdm.getBy({TabId:'20'})
    const expected = [{
        "Name": "Home",
        "TabId": "20",
        "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Home.png",
        "Tooltip": "Homepage of the site",
        "ParentTabId": -1,
        "HasChildren": true,
        "IsOpen": false,
        "Selectable": true,
        "CheckedState": 0,
        "ChildTabs": ['211-Inner Child', '21111-Inner Inner Child']
      }]

    expect(result).toEqual(expected)

})

test.skip('_mapRootTabs() will return the root tab that contains the ChildTab in subset collection', () => {
    ppdm.flatten(TabSubset)
    const result = ppdm._mapRootTabs('222-Inner Inner Child 2')
    const expected = {
      "Name": "Home",
      "TabId": "20",
      "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Home.png",
      "Tooltip": "Homepage of the site",
      "ParentTabId": 0,
      "HasChildren": true,
      "IsOpen": false,
      "Selectable": true,
      "CheckedState": 0,
      "ChildTabs" : ["211-Inner Child", "21111-Inner Inner Child", "222-Inner Inner Child 2"]
    }

    expect(result).toEqual(expected)
})

test.skip('_mapRootTabs() will return the root tab that contains the ChildTab in full page collection', () => {
    ppdm.flatten(Tabs)
    const result = ppdm._mapRootTabs('2053-Inner Inner Child')
    const expected =  { Name: 'My Website',
      TabId: '-1',
      ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Portal.png',
      Tooltip: null,
      ParentTabId: 0,
      HasChildren: true,
      IsOpen: false,
      Selectable: true,
      CheckedState: 0,
      ChildTabs:
       [ '20-Home',
         '21-Our Products',
         '211-Inner Child',
         '212-Inner Child 2',
         '22-Activity Feed',
         '23-News',
         '24-Community',
         '25-Search Results',
         '26-404 Error Page' ] }


    console.log(result)

    expect(result).toEqual(expected)
})

test.skip("[subset data]:: get children of a parent tab and update checked state of children including parent:: FULL SELECT", ()=>{
    ppdm.flatten(TabSubset)
    const children = ppdm.childrenOf({id:'20-Home'})
    children.forEach( (tab) => ppdm.update({id:`${tab.TabId}-${tab.Name}`}, {CheckedState:1}) )

    const result = ppdm.export()
    const expected = {
        "20-Home" : {
          "Name": "Home",
          "TabId": "20",
          "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Home.png",
          "Tooltip": "Homepage of the site",
          "ParentTabId": -1,
          "HasChildren": true,
          "IsOpen": false,
          "Selectable": true,
          "CheckedState": 1,
          "ChildTabs": ['211-Inner Child', '21111-Inner Inner Child']
        },
        "211-Inner Child": {
          "Name": "Inner Child",
          "TabId": "211",
          "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Everyone.png",
          "Tooltip": "Page is visible to everyone",
          "ParentTabId": 20,
          "HasChildren": true,
          "IsOpen": false,
          "Selectable": true,
          "CheckedState": 1,
          "ChildTabs": ['21111-Inner Inner Child']
        },
        '21111-Inner Inner Child':{
          "Name": "Inner Inner Child",
          "TabId": "21111",
          "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Everyone.png",
          "Tooltip": "Page is visible to everyone",
          "ParentTabId": 211,
          "HasChildren": true,
          "IsOpen": false,
          "Selectable": true,
          "CheckedState": 1,
        }
    }

    console.log(result)
    expect(result).toEqual(expected)

})

test.skip("[subset data]:: get children of a parent tab and update checked state of children including parent:: PARTIAL SELECT", ()=>{
    ppdm.flatten(TabSubset)
    const children = ppdm.childrenOf({id:'20-Home'})
    const childTab = children[0]

    ppdm.update({id:`${childTab.TabId}-${childTab.Name}`}, {CheckedState:1})
    const result = ppdm.export()
    const expected =  {
        "20-Home" : {
          "Name": "Home",
          "TabId": "20",
          "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Home.png",
          "Tooltip": "Homepage of the site",
          "ParentTabId": -1,
          "HasChildren": true,
          "IsOpen": false,
          "Selectable": true,
          "CheckedState": -1,
          "ChildTabs": ['211-Inner Child', '21111-Inner Inner Child']
        },
        "211-Inner Child": {
          "Name": "Inner Child",
          "TabId": "211",
          "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Everyone.png",
          "Tooltip": "Page is visible to everyone",
          "ParentTabId": 20,
          "HasChildren": true,
          "IsOpen": false,
          "Selectable": true,
          "CheckedState": 1,
          "ChildTabs": ['21111-Inner Inner Child']
        },
        '21111-Inner Inner Child':{
          "Name": "Inner Inner Child",
          "TabId": "21111",
          "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Everyone.png",
          "Tooltip": "Page is visible to everyone",
          "ParentTabId": 211,
          "HasChildren": true,
          "IsOpen": false,
          "Selectable": true,
          "CheckedState": 0,
        }
    }

    console.log(result)
    expect(result).toEqual(expected)
})

test.skip("[subset data]:: get children of a parent tab and update checked state of children including parent:: UNSELECT", ()=>{
    ppdm.flatten(TabSubset)
    const children = ppdm.childrenOf({id:'20-Home'})
    const childTab = children[0]
    ppdm.update({id:`${childTab.TabId}-${childTab.Name}`}, {CheckedState:0})
    const result = ppdm.export()

    const expected =  {
        "20-Home" : {
          "Name": "Home",
          "TabId": "20",
          "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Home.png",
          "Tooltip": "Homepage of the site",
          "ParentTabId": -1,
          "HasChildren": true,
          "IsOpen": false,
          "Selectable": true,
          "CheckedState": 0,
          "ChildTabs": ['211-Inner Child', '21111-Inner Inner Child']
        },
        "211-Inner Child": {
          "Name": "Inner Child",
          "TabId": "211",
          "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Everyone.png",
          "Tooltip": "Page is visible to everyone",
          "ParentTabId": 20,
          "HasChildren": true,
          "IsOpen": false,
          "Selectable": true,
          "CheckedState": 0,
          "ChildTabs": ['21111-Inner Inner Child']
        },
        '21111-Inner Inner Child':{
          "Name": "Inner Inner Child",
          "TabId": "21111",
          "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Everyone.png",
          "Tooltip": "Page is visible to everyone",
          "ParentTabId": 211,
          "HasChildren": true,
          "IsOpen": false,
          "Selectable": true,
          "CheckedState": 0,
        }
    }

    console.log(result)
    expect(result).toEqual(expected)
})

test.skip("[complete dataset]:: get children of a parent tab and update checked state of children including parent:: FULL SELECT", ()=>{
    ppdm.flatten(Tabs)
    const children = ppdm.childrenOf({id:'23-My Website'})
    children.forEach( (tab) => ppdm.update({id:`${tab.TabId}-${tab.Name}`}, {CheckedState:1}) )

    const result = ppdm.export()
    const expected =    { '23-My Website':
       { Name: 'My Website',
         TabId: 23,
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Portal.png',
         Tooltip: null,
         ParentTabId: 0,
         HasChildren: true,
         IsOpen: false,
         Selectable: true,
         CheckedState: 1,
         ChildTabs:
          [ '20-Home',
            '201-Inner Child 1',
            '20A-Inner Inner Child 1',
            '21-Activity Feed',
            '21A-Inner Child 2',
            '21AB-Inner Inner Child 2',
            '22-Search Results',
            '23-404 Error Page' ] },
      '20-Home':
       { Name: 'Home',
         TabId: '20',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
         Tooltip: 'Homepage of the site',
         ParentTabId: -1,
         HasChildren: true,
         IsOpen: false,
         Selectable: true,
         CheckedState: 1,
         ChildTabs: [ '201-Inner Child 1', '20A-Inner Inner Child 1' ] },
      '21-Activity Feed':
       { Name: 'Activity Feed',
         TabId: '21',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Everyone.png',
         Tooltip: 'Page is visible to everyone',
         ParentTabId: -1,
         HasChildren: true,
         IsOpen: false,
         Selectable: true,
         CheckedState: 1,
         ChildTabs: [ '21A-Inner Child 2', '21AB-Inner Inner Child 2' ] },
      '22-Search Results':
       { Name: 'Search Results',
         TabId: '22',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Everyone.png',
         Tooltip: 'Page is visible to everyone',
         ParentTabId: -1,
         HasChildren: false,
         IsOpen: false,
         Selectable: true,
         CheckedState: 1,
         ChildTabs: [] },
      '23-404 Error Page':
       { Name: '404 Error Page',
         TabId: '23',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Everyone.png',
         Tooltip: 'Page is visible to everyone',
         ParentTabId: -1,
         HasChildren: false,
         IsOpen: false,
         Selectable: true,
         CheckedState: 1,
         ChildTabs: [] },
      '201-Inner Child 1':
       { Name: 'Inner Child 1',
         TabId: '201',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
         Tooltip: 'Homepage of the site',
         ParentTabId: 20,
         HasChildren: true,
         IsOpen: false,
         Selectable: true,
         CheckedState: 1,
         ChildTabs: [ '20A-Inner Inner Child 1' ] },
      '21A-Inner Child 2':
       { Name: 'Inner Child 2',
         TabId: '21A',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
         Tooltip: 'Homepage of the site',
         ParentTabId: 21,
         HasChildren: true,
         IsOpen: false,
         Selectable: true,
         CheckedState: 1,
         ChildTabs: [ '21AB-Inner Inner Child 2' ] },
      '20A-Inner Inner Child 1':
       { Name: 'Inner Inner Child 1',
         TabId: '20A',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
         Tooltip: 'Homepage of the site',
         ParentTabId: '2011',
         HasChildren: false,
         IsOpen: false,
         Selectable: true,
         CheckedState: 1,
         ChildTabs: [] },
      '21AB-Inner Inner Child 2':
       { Name: 'Inner Inner Child 2',
         TabId: '21AB',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
         Tooltip: 'Homepage of the site',
         ParentTabId: '21A',
         HasChildren: false,
         IsOpen: false,
         Selectable: true,
         CheckedState: 1,
         ChildTabs: [] } }


    console.log(result)
    expect(result).toEqual(expected)

})

test.skip("[complete dataset]:: select a parent child and select all its children, root component should be partial and parent child fulll select :: PARTIAL SELECT", ()=>{
    ppdm.flatten(Tabs)
    const children = ppdm.childrenOf({id:'20-Home'})
    children.forEach( (tab) => ppdm.update({id:`${tab.TabId}-${tab.Name}`}, {CheckedState:1}) )

    const result = ppdm.export()
    const expected =   { '23-My Website':
       { Name: 'My Website',
         TabId: 23,
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Portal.png',
         Tooltip: null,
         ParentTabId: 0,
         HasChildren: true,
         IsOpen: false,
         Selectable: true,
         CheckedState: -1,
         ChildTabs:
          [ '20-Home',
            '201-Inner Child 1',
            '20A-Inner Inner Child 1',
            '21-Activity Feed',
            '21A-Inner Child 2',
            '21AB-Inner Inner Child 2',
            '22-Search Results',
            '23-404 Error Page' ] },
      '20-Home':
       { Name: 'Home',
         TabId: '20',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
         Tooltip: 'Homepage of the site',
         ParentTabId: -1,
         HasChildren: true,
         IsOpen: false,
         Selectable: true,
         CheckedState: 1,
         ChildTabs: [ '201-Inner Child 1', '20A-Inner Inner Child 1' ] },
      '21-Activity Feed':
       { Name: 'Activity Feed',
         TabId: '21',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Everyone.png',
         Tooltip: 'Page is visible to everyone',
         ParentTabId: -1,
         HasChildren: true,
         IsOpen: false,
         Selectable: true,
         CheckedState: 0,
         ChildTabs: [ '21A-Inner Child 2', '21AB-Inner Inner Child 2' ] },
      '22-Search Results':
       { Name: 'Search Results',
         TabId: '22',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Everyone.png',
         Tooltip: 'Page is visible to everyone',
         ParentTabId: -1,
         HasChildren: false,
         IsOpen: false,
         Selectable: true,
         CheckedState: 0,
         ChildTabs: [] },
      '23-404 Error Page':
       { Name: '404 Error Page',
         TabId: '23',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Everyone.png',
         Tooltip: 'Page is visible to everyone',
         ParentTabId: -1,
         HasChildren: false,
         IsOpen: false,
         Selectable: true,
         CheckedState: 0,
         ChildTabs: [] },
      '201-Inner Child 1':
       { Name: 'Inner Child 1',
         TabId: '201',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
         Tooltip: 'Homepage of the site',
         ParentTabId: 20,
         HasChildren: true,
         IsOpen: false,
         Selectable: true,
         CheckedState: 1,
         ChildTabs: [ '20A-Inner Inner Child 1' ] },
      '21A-Inner Child 2':
       { Name: 'Inner Child 2',
         TabId: '21A',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
         Tooltip: 'Homepage of the site',
         ParentTabId: 21,
         HasChildren: true,
         IsOpen: false,
         Selectable: true,
         CheckedState: 0,
         ChildTabs: [ '21AB-Inner Inner Child 2' ] },
      '20A-Inner Inner Child 1':
       { Name: 'Inner Inner Child 1',
         TabId: '20A',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
         Tooltip: 'Homepage of the site',
         ParentTabId: '2011',
         HasChildren: false,
         IsOpen: false,
         Selectable: true,
         CheckedState: 1,
         ChildTabs: [] },
      '21AB-Inner Inner Child 2':
       { Name: 'Inner Inner Child 2',
         TabId: '21AB',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
         Tooltip: 'Homepage of the site',
         ParentTabId: '21A',
         HasChildren: false,
         IsOpen: false,
         Selectable: true,
         CheckedState: 0,
         ChildTabs: [] } }

    console.log(result)
    expect(result).toEqual(expected)

})

test.skip("[complete dataset]:: get children of a parent tab and update checked state of children including parent:: PARTIAL SELECT", ()=>{
    ppdm.flatten(Tabs)
    const children = ppdm.childrenOf({id:'20-Home'})
    const childTab = children[0]

    ppdm.update({id:`${childTab.TabId}-${childTab.Name}`}, {CheckedState:1})
    const result = ppdm.export()
    const expected =   { '23-My Website':
       { Name: 'My Website',
         TabId: 23,
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Portal.png',
         Tooltip: null,
         ParentTabId: 0,
         HasChildren: true,
         IsOpen: false,
         Selectable: true,
         CheckedState: -1,
         ChildTabs:
          [ '20-Home',
            '201-Inner Child 1',
            '20A-Inner Inner Child 1',
            '21-Activity Feed',
            '21A-Inner Child 2',
            '21AB-Inner Inner Child 2',
            '22-Search Results',
            '23-404 Error Page' ] },
      '20-Home':
       { Name: 'Home',
         TabId: '20',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
         Tooltip: 'Homepage of the site',
         ParentTabId: -1,
         HasChildren: true,
         IsOpen: false,
         Selectable: true,
         CheckedState: -1,
         ChildTabs: [ '201-Inner Child 1', '20A-Inner Inner Child 1' ] },
      '21-Activity Feed':
       { Name: 'Activity Feed',
         TabId: '21',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Everyone.png',
         Tooltip: 'Page is visible to everyone',
         ParentTabId: -1,
         HasChildren: true,
         IsOpen: false,
         Selectable: true,
         CheckedState: 0,
         ChildTabs: [ '21A-Inner Child 2', '21AB-Inner Inner Child 2' ] },
      '22-Search Results':
       { Name: 'Search Results',
         TabId: '22',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Everyone.png',
         Tooltip: 'Page is visible to everyone',
         ParentTabId: -1,
         HasChildren: false,
         IsOpen: false,
         Selectable: true,
         CheckedState: 0,
         ChildTabs: [] },
      '23-404 Error Page':
       { Name: '404 Error Page',
         TabId: '23',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Everyone.png',
         Tooltip: 'Page is visible to everyone',
         ParentTabId: -1,
         HasChildren: false,
         IsOpen: false,
         Selectable: true,
         CheckedState: 0,
         ChildTabs: [] },
      '201-Inner Child 1':
       { Name: 'Inner Child 1',
         TabId: '201',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
         Tooltip: 'Homepage of the site',
         ParentTabId: 20,
         HasChildren: true,
         IsOpen: false,
         Selectable: true,
         CheckedState: 1,
         ChildTabs: [ '20A-Inner Inner Child 1' ] },
      '21A-Inner Child 2':
       { Name: 'Inner Child 2',
         TabId: '21A',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
         Tooltip: 'Homepage of the site',
         ParentTabId: 21,
         HasChildren: true,
         IsOpen: false,
         Selectable: true,
         CheckedState: 0,
         ChildTabs: [ '21AB-Inner Inner Child 2' ] },
      '20A-Inner Inner Child 1':
       { Name: 'Inner Inner Child 1',
         TabId: '20A',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
         Tooltip: 'Homepage of the site',
         ParentTabId: '2011',
         HasChildren: false,
         IsOpen: false,
         Selectable: true,
         CheckedState: 0,
         ChildTabs: [] },
      '21AB-Inner Inner Child 2':
       { Name: 'Inner Inner Child 2',
         TabId: '21AB',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
         Tooltip: 'Homepage of the site',
         ParentTabId: '21A',
         HasChildren: false,
         IsOpen: false,
         Selectable: true,
         CheckedState: 0,
         ChildTabs: [] } }


    console.log(result)
    expect(result).toEqual(expected)
})

test.skip("[subset data]:: get children of a parent tab and update checked state of children including parent:: UNSELECT", ()=>{
    ppdm.flatten(TabRootOnlySelected)
    const children = ppdm.childrenOf({id:'20-Home'})
    const childTab = children[0]

    ppdm.update({id:`${childTab.TabId}-${childTab.Name}`}, {CheckedState:0})
    const result = ppdm.export()
    const expected =   { '23-My Website':
       { Name: 'My Website',
         TabId: 23,
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Portal.png',
         Tooltip: null,
         ParentTabId: 0,
         HasChildren: true,
         IsOpen: false,
         Selectable: true,
         CheckedState: 1,
         ChildTabs:
          [ '20-Home',
            '201-Inner Child 1',
            '20A-Inner Inner Child 1',
            '21-Activity Feed',
            '21A-Inner Child 2',
            '21AB-Inner Inner Child 2',
            '22-Search Results',
            '23-404 Error Page' ] },
      '20-Home':
       { Name: 'Home',
         TabId: '20',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
         Tooltip: 'Homepage of the site',
         ParentTabId: -1,
         HasChildren: true,
         IsOpen: false,
         Selectable: true,
         CheckedState: 0,
         ChildTabs: [ '201-Inner Child 1', '20A-Inner Inner Child 1' ] },
      '21-Activity Feed':
       { Name: 'Activity Feed',
         TabId: '21',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Everyone.png',
         Tooltip: 'Page is visible to everyone',
         ParentTabId: -1,
         HasChildren: true,
         IsOpen: false,
         Selectable: true,
         CheckedState: 0,
         ChildTabs: [ '21A-Inner Child 2', '21AB-Inner Inner Child 2' ] },
      '22-Search Results':
       { Name: 'Search Results',
         TabId: '22',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Everyone.png',
         Tooltip: 'Page is visible to everyone',
         ParentTabId: -1,
         HasChildren: false,
         IsOpen: false,
         Selectable: true,
         CheckedState: 0,
         ChildTabs: [] },
      '23-404 Error Page':
       { Name: '404 Error Page',
         TabId: '23',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Everyone.png',
         Tooltip: 'Page is visible to everyone',
         ParentTabId: -1,
         HasChildren: false,
         IsOpen: false,
         Selectable: true,
         CheckedState: 0,
         ChildTabs: [] },
      '201-Inner Child 1':
       { Name: 'Inner Child 1',
         TabId: '201',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
         Tooltip: 'Homepage of the site',
         ParentTabId: 20,
         HasChildren: true,
         IsOpen: false,
         Selectable: true,
         CheckedState: 0,
         ChildTabs: [ '20A-Inner Inner Child 1' ] },
      '21A-Inner Child 2':
       { Name: 'Inner Child 2',
         TabId: '21A',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
         Tooltip: 'Homepage of the site',
         ParentTabId: 21,
         HasChildren: true,
         IsOpen: false,
         Selectable: true,
         CheckedState: 0,
         ChildTabs: [ '21AB-Inner Inner Child 2' ] },
      '20A-Inner Inner Child 1':
       { Name: 'Inner Inner Child 1',
         TabId: '20A',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
         Tooltip: 'Homepage of the site',
         ParentTabId: '2011',
         HasChildren: false,
         IsOpen: false,
         Selectable: true,
         CheckedState: 0,
         ChildTabs: [] },
      '21AB-Inner Inner Child 2':
       { Name: 'Inner Inner Child 2',
         TabId: '21AB',
         ImageUrl: '/DesktopModules/Admin/Tabs/images/Icon_Home.png',
         Tooltip: 'Homepage of the site',
         ParentTabId: '21A',
         HasChildren: false,
         IsOpen: false,
         Selectable: true,
         CheckedState: 0,
         ChildTabs: [] } }

    console.log(result)
    expect(result).toEqual(expected)
})
