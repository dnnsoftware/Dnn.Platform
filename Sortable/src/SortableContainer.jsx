import React, {Component, PropTypes} from "react";
import Droppable from "./Droppable";
import SortableItem from "./SortableItem";

class SortableContainer extends Component {
    render() {
        const sortableList = this.props.children.map((child, index) => {
            return <SortableItem
                cloneElementOnDrag={true}
                key={index}
                onDragMove={this.props.onDragMove}
                onDragStart={this.props.onDragStart}
                onDragEnd={this.props.onDragEnd }
                >
                {child}
            </SortableItem>;
        });
        return <div>
            {sortableList}
        </div>;
    }
}

SortableContainer.propTypes = {
    children: PropTypes.node,
    isDragging: PropTypes.bool.isRequired,
    isDraggingOver: PropTypes.bool.isRequired,
    onDragStart: PropTypes.func.isRequired,
    onDragEnd: PropTypes.func.isRequired,
    onDragMove: PropTypes.func.isRequired,
    onDrop: PropTypes.func.isRequired,
    onDropMove: PropTypes.func.isRequired
};

export default Droppable(SortableContainer);