import React, {Component} from "react";
import { PropTypes } from "prop-types";
import "./styles.less";
import {PagesIcon, FolderIcon} from "dnn-svg-icons";

export default class PersonaBarPageIcon extends Component {

    selectIcon(number) {
        // 1. PagesIcon
        // 2. FolderIcon
        // 3. RightLink
        // 4. LeftLink

        switch(number) {
            case 1:
               return (<div dangerouslySetInnerHTML={{ __html: PagesIcon }} />);

            case 2:
                return (<div dangerouslySetInnerHTML={{ __html: FolderIcon }} />);

        }
    }

    render() {
        return (
            <div className="dnn-persona-bar-treeview-icon">
                {this.selectIcon(this.props.iconType)}
            </div>
        );
    }

}

PersonaBarPageIcon.propTypes = {
    iconType: PropTypes.number.isRequired
};