import React, {Component} from "react";
import { PropTypes } from "prop-types";

import {TreeAddPage,TreeAnalytics, TreeCopy, TreeEdit, TreeEye } from "dnn-svg-icons";



import "./styles.less";

export class PersonaBarTreeInContextMenu extends Component {


    cloak(){

    }

    render_default(item){
        return(
            <div className="in-context-menu"></div>
        );
    }

    render_actionable(item) {
        const cloak = !item.selected || !item.hasOwnProperty('showInContextMenu') ? {visibility:"hidden"} : {visibility:"visible"};

        return(
            <div
                id={`context-menu-${item.name}-${item.id}`}
                className={item.selected && item.showInContextMenu? "in-context-menu active" : `in-context-menu inactive`}
                style={cloak}
                >

                <ul>
                    <li>
                        <div className="icon" dangerouslySetInnerHTML={{__html:TreeAddPage}}/>
                        <div className="label">Add Page</div>
                    </li>
                    <li>
                        <div className="icon" dangerouslySetInnerHTML={{__html:TreeEye}}/>
                        <div className="label">View</div>
                    </li>
                    <li>
                        <div className="icon" dangerouslySetInnerHTML={{__html:TreeEdit}}/>
                        <div className="label">Edit</div>
                    </li>
                    <li>
                        <div className="icon" dangerouslySetInnerHTML={{__html:TreeCopy}}/>
                        <div className="label">Duplicate</div>
                    </li>
                    <li>
                        <div className="icon" dangerouslySetInnerHTML={{__html:TreeAnalytics}}/>
                        <div className="label">Analytics</div>
                    </li>
                </ul>
            </div> );
    }

    render(){
        const {item} = this.props;
        return(
            <span>
               { this.render_actionable(item) }
            </span>
        );
    }

}

PersonaBarTreeInContextMenu.propTypes = {
    item: PropTypes.object.isRequired
};


