export const tabs =  {
        "Name": "My Website",
        "TabId": 23,
        "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Portal.png",
        "Tooltip": null,
        "ParentTabId": 0,
        "HasChildren": true,
        "IsOpen": false,
        "Selectable": true,
        "CheckedState": 1,
        "ChildTabs": [{
            "Name": "Home",
            "TabId": "20",
            "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Home.png",
            "Tooltip": "Homepage of the site",
            "ParentTabId": -1,
            "HasChildren": true,
            "IsOpen": false,
            "Selectable": true,
            "CheckedState": -1,
            "ChildTabs": [
              {
                  "Name": "Inner Child 1",
                  "TabId": "201",
                  "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Home.png",
                  "Tooltip": "Homepage of the site",
                  "ParentTabId": 20,
                  "HasChildren": true,
                  "IsOpen": false,
                  "Selectable": true,
                  "CheckedState": 1,
                  "ChildTabs": [
                    {
                        "Name": "Inner Inner Child 1",
                        "TabId": "20A",
                        "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Home.png",
                        "Tooltip": "Homepage of the site",
                        "ParentTabId": '2011',
                        "HasChildren": false,
                        "IsOpen": false,
                        "Selectable": true,
                        "CheckedState": 0,
                        "ChildTabs": []
                    }
                  ]
              }
            ]
        }, {
            "Name": "Activity Feed",
            "TabId": "21",
            "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Everyone.png",
            "Tooltip": "Page is visible to everyone",
            "ParentTabId": -1,
            "HasChildren": true,
            "IsOpen": false,
            "Selectable": true,
            "CheckedState": 0,
            "ChildTabs": [
              {
                  "Name": "Inner Child 2",
                  "TabId": "21A",
                  "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Home.png",
                  "Tooltip": "Homepage of the site",
                  "ParentTabId": 21,
                  "HasChildren": true,
                  "IsOpen": false,
                  "Selectable": true,
                  "CheckedState": 0,
                  "ChildTabs": [
                    {
                        "Name": "Inner Inner Child 2",
                        "TabId": "21AB",
                        "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Home.png",
                        "Tooltip": "Homepage of the site",
                        "ParentTabId": '21A',
                        "HasChildren": false,
                        "IsOpen": false,
                        "Selectable": true,
                        "CheckedState": 0,
                        "ChildTabs": []
                    }
                  ]
              }
            ]
        }, {
            "Name": "Search Results",
            "TabId": "22",
            "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Everyone.png",
            "Tooltip": "Page is visible to everyone",
            "ParentTabId": -1,
            "HasChildren": false,
            "IsOpen": false,
            "Selectable": true,
            "CheckedState": 0,
            "ChildTabs": []
        }, {
            "Name": "404 Error Page",
            "TabId": "23",
            "ImageUrl": "/DesktopModules/Admin/Tabs/images/Icon_Everyone.png",
            "Tooltip": "Page is visible to everyone",
            "ParentTabId": -1,
            "HasChildren": false,
            "IsOpen": false,
            "Selectable": true,
            "CheckedState": 0,
            "ChildTabs": []
        }]
    }
