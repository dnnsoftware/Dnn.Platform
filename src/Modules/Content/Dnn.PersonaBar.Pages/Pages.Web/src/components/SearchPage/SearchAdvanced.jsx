import React, { Component } from "react";
import Collapse from "dnn-collapsible";
import { SearchIcon } from "dnn-svg-icons";
import Localization from "../../localization";
import SearchAdvancedDetails from "./SearchAdvancedDetails";
import "./styles.less";


export default class SearchAdvanced extends Component {
    constructor(props) {
        super(props);
        this.state = {
            collapsed:false
        };
    }

    toggle() {
        this.setState({
            collapsed:!this.state.collapsed
        });
    }

    /*eslint-disable react/no-danger*/
    render() {
        return (
            <div className={`advancedCollapsibleComponent ${this.state.collapsed?"open":""}`}>
                <div className={`collapsible-header false` } onClick={this.toggle.bind(this)}>
                    <div className="search-advanced-header">
                        <div className="search-advanced-icon" dangerouslySetInnerHTML={{ __html: SearchIcon }}>
                        </div>
                        <div className="search-advanced-label">
                            {Localization.get("AdvancedFilters")}
                        </div>
                    </div>
                    <span
                        className={`collapse-icon ${this.state.collapsed?"collapsed":""}`}>
                    </span>
                </div>
                <Collapse isOpened={this.state.collapsed} className="search-header-collapsible">
                    <div style={{height:"150px", padding: "25px"}}>
                        <div className="">TODO</div>
                    </div>
                </Collapse>
                
            </div>
        );
    }
}