import React, { Component } from "react";
import PropTypes from "prop-types";
import Collapse from "dnn-collapsible";
import { SimpleType } from "dnn-svg-icons";


export default class Search extends Component {
    constructor(props){
        super(props);
    }

    /*eslint-disable react/no-danger*/
    render() {
        return (
            <div>
                <div className={"collapsible-header "}>
                    <div className="term-header">
                        <div className="term-icon" dangerouslySetInnerHTML={{ __html: SimpleType }}>
                        </div>
                        <div className="term-label">
                             {/* TODO - Replace with localizations */}
                            ADVANCED FILTERS
                        </div>
                    </div>
                    <span
                        className={"collapse-icon collapsed"}>
                    </span>
                </div>
                <Collapse>
                    <div>TODO FILTERS.....</div>
                </Collapse>
                
            </div>
        );
    }
}


