export const PortalTabs =  [{
        "Name": "My Website",
        "TabId": "-1",
        "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Portal.png",
        "Tooltip": null,
        "ParentTabId": "0",
        "HasChildren": true,
        "IsOpen": false,
        "Selectable": true,
        "CheckedState": 2,
        "ChildTabs": [{
            "Name": "Home",
            "TabId": "20",
            "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Home.png",
            "Tooltip": "Homepage of the site",
            "ParentTabId": "-1",
            "HasChildren": true,
            "IsOpen": false,
            "Selectable": true,
            "CheckedState": 2,
            "ChildTabs": [
              {
                    "Name": "Inner Child 1",
                    "TabId": "201",
                    "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Home.png",
                    "Tooltip": "Homepage of the site",
                    "ParentTabId": "20",
                    "HasChildren": false,
                    "IsOpen": false,
                    "Selectable": true,
                    "CheckedState": 1,
                    "ChildTabs": []
                },
                {
                  "Name": "Inner Child 2",
                  "TabId": "202",
                  "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Home.png",
                  "Tooltip": "Homepage of the site",
                  "ParentTabId": "20",
                  "HasChildren": true,
                  "IsOpen": false,
                  "Selectable": true,
                  "CheckedState": 2,
                  "ChildTabs": [
                    {
                        "Name": "Inner Inner Child 1",
                        "TabId": "2011",
                        "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Home.png",
                        "Tooltip": "Homepage of the site",
                        "ParentTabId": '202',
                        "HasChildren": false,
                        "IsOpen": false,
                        "Selectable": true,
                        "CheckedState": 1,
                        "ChildTabs": []
                    }
                    ,{
                        "Name": "Inner Inner Sib 2",
                        "TabId": "2012",
                        "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Home.png",
                        "Tooltip": "Homepage of the site",
                        "ParentTabId": '201',
                        "HasChildren": false,
                        "IsOpen": false,
                        "Selectable": true,
                        "CheckedState": 1,
                        "ChildTabs": []
                    }
                  ]
              }]
        }
        ,{
            "Name": "Activity Feed",
            "TabId": "21",
            "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Everyone.png",
            "Tooltip": "Page is visible to everyone",
            "ParentTabId": "-1",
            "HasChildren": true,
            "IsOpen": false,
            "Selectable": true,
            "CheckedState": 2,
            "ChildTabs": [
              {
                  "Name": "Inner Child 2",
                  "TabId": "211",
                  "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Home.png",
                  "Tooltip": "Homepage of the site",
                  "ParentTabId": "21",
                  "HasChildren": true,
                  "IsOpen": false,
                  "Selectable": true,
                  "CheckedState": 1,
                  "ChildTabs": [
                    {
                        "Name": "Inner Inner Child 2",
                        "TabId": "2111",
                        "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Home.png",
                        "Tooltip": "Homepage of the site",
                        "ParentTabId": '211',
                        "HasChildren": false,
                        "IsOpen": false,
                        "Selectable": true,
                        "CheckedState": 1,
                        "ChildTabs": []
                    }
                  ]
              },{
                  "Name": "Inner Child Sibling",
                  "TabId": "212",
                  "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Home.png",
                  "Tooltip": "Homepage of the site",
                  "ParentTabId": "21",
                  "HasChildren": true,
                  "IsOpen": false,
                  "Selectable": true,
                  "CheckedState": 2,
                  "ChildTabs": [
                    {
                        "Name": "Inner Inner Child 2",
                        "TabId": "2121",
                        "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Home.png",
                        "Tooltip": "Homepage of the site",
                        "ParentTabId": '212',
                        "HasChildren": false,
                        "IsOpen": false,
                        "Selectable": true,
                        "CheckedState": 1,
                        "ChildTabs": []
                    }
                  ]
              }
            ]
        },{
            "Name": "Search Results",
            "TabId": "22",
            "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Everyone.png",
            "Tooltip": "Page is visible to everyone",
            "ParentTabId": "-1",
            "HasChildren": false,
            "IsOpen": false,
            "Selectable": true,
            "CheckedState": 1,
            "ChildTabs": []
        },{
            "Name": "404 Error Page",
            "TabId": "23",
            "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Everyone.png",
            "Tooltip": "Page is visible to everyone",
            "ParentTabId": "-1",
            "HasChildren": false,
            "IsOpen": false,
            "Selectable": true,
            "CheckedState": 1,
            "ChildTabs": []
        }
      ]
    }]
