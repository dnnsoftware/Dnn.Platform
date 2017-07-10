import React, {Component} from "react";
import {PersonaBarPageTreeview} from "./PersonaBarPageTreeview";

export class PersonaBarPageTreeviewInteractor extends Component {

    constructor(){
        super();
        this.origin = window.origin;
        //http://auto.engage14-162.com/API/PersonaBar/EvoqPages/GetPageList?searchKey=

        this.state = {};
        this.origin = window.origin;
        this.url = `${window.origin}/API/PersonaBar/${window.dnn.pages.apiController}/GetPageList?searchKey=`;
        this.GET(this.url, this.setState.bind(this));

    }

    GET(url, setState) {
        function reqListener () {
           const pageList =  JSON.parse(this.responseText);
           setState({pageList: pageList}, function(){
               console.log(this.state);
           });
        }

        const xhr= new XMLHttpRequest();
        xhr.addEventListener("load", reqListener);
        xhr.open("GET", url);
        xhr.send();
    }

    requestRootPageList(){

    }

    render() {

        return (
            <PersonaBarPageTreeview />
        );
    }
}