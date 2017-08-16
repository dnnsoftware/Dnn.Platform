import React, {Component} from "react";
import { PropTypes } from "prop-types";

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
                className={item.selected && item.showInContextMenu? "in-context-menu active" : `in-context-menu inactive ${cloak}`}
                style={cloak}
                >

                <ul>
                    <li>Item One</li>
                    <li>Item two</li>
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


