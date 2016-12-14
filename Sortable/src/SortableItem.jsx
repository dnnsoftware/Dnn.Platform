import React, {Component, PropTypes} from "react";
import Draggable from "./Draggable";

class SortableItem extends Component {
    render() {
        const item = this.props.children.props.listItem;
        const selected = this.props.children.props.selected;

        const className = "sortable-item drag-element" + (selected ? " sortable-selected" : "");
        return <div className={className} data-index={item.index} data-dnn-sortable-id={item.id}>
            {this.props.children}
        </div>;
    }
}

SortableItem.propTypes = {
    children: PropTypes.node
};

export default Draggable(SortableItem);
