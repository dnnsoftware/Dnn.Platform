import React, {Component, PropTypes} from "react";
import SortableContainer from "./SortableContainer";
import ReactDOM from "react-dom";
import "./style.less";

export default class Sortable extends Component {
    constructor(props) {
        super(props);

        const items = this.props.items.map((item, index) => {
            return { index, item, id: index };
        });
        this.currentIndex = -1;
        this.state = {
            items,
            dragging: {
                isDragging: false
            },
            isDraggingOver: false
        };
    }

    componentWillReceiveProps(newProps) {
        const items = newProps.items.map((item, index) => {
            return { index, item, id: index };
        });
        this.setState({items});
    }

    sortColumns(a, b) {
        if (a.index < b.index)
        { return -1; }
        else if (a.index > b.index)
        { return 1; }
        return 0;
    }

    onDragStart(component, event) {
        const itemElement = ReactDOM.findDOMNode(this.refs.dnnSortable).querySelectorAll(`[data-dnn-sortable-id="${id}"]`)[0];

        let dragging = {
            isDragging: true
        };
        this.setState({ dragging });
        const id = event.target.getAttribute("data-dnn-sortable-id");
        this.setFieldSelected(id, true);
        event.target.classList.add("dnn-sortable-dragging");
    }

    onDragEnd(component, event) {
        let dragging = {
            isDragging: false
        };
        this.setState({ dragging });
        const id = event.target.getAttribute("data-dnn-sortable-id");
        this.setFieldSelected(id, false);
        this.currentIndex = -1;
        this.unselectAll();
    }

    unselectAll() {
        let {items} = this.state;
        items.forEach(i => i ? i.selected = false : false);
        this.setState({ items });
    }

    setFieldSelected(id, selected) {
        let {items} = this.state;

        items.forEach((items) => {
            if (items.id == id) {
                items.selected = selected;
            } else {
                items.selected = false;
            }
        });
        // const itemElement = ReactDOM.findDOMNode(this.refs.dnnSortable).querySelectorAll(`[data-dnn-sortable-id="${id}"]`)[0];
        // if (selected) {
        //     return itemElement.classList.add("sortable-selected");
        // }
        // itemElement.classList.remove("sortable-selected");
    }

    onDragMove() {
    }

    updateItemClass(sortableItems, dropY) {
        [].forEach.call(sortableItems, (item, index) => {
            if (dropY > item.offsetHeight * index + item.getBoundingClientRect().height / 2 &&
                dropY < item.offsetHeight * index + item.getBoundingClientRect().height + item.getBoundingClientRect().height / 2) {
                item.classList.add("dragged-over");
            } else {
                item.classList.remove("dragged-over");
            }
        });
    }

    onDropMove(event, dropX, dropY) {
        this.setState({ isDraggingOver: true });
        const sortableItems = event.target.getElementsByClassName("sortable-item");
        if (this.props.sortOnDrag) {
            this.sortOnDrag(event, dropX, dropY);
        } else {
            this.updateItemClass(sortableItems, dropY);
        }
    }

    onDragLeave() {
        this.setState({ isDraggingOver: false });
        this.removePlaceholder();
    }

    removePlaceholder() {
        const sortableItems = document.getElementsByClassName("sortable-item");
        this.updateItemClass(sortableItems, -1000);
    }

    getNewIndex(sortableItems, dropY) {
        let newIndex = 0;
        [].forEach.call(sortableItems, (item, index) => {
            if (dropY > item.offsetHeight * index + item.getBoundingClientRect().height / 2) {
                newIndex = +item.getAttribute("data-index") + 1;
            }
        });
        return newIndex;
    }

    sortItem(newIndex) {
        const curIndex = this.currentIndex;
        let {items} = this.state;
        let currentItem = items.find(i => i.index == curIndex);
        let itemToReplace = items.find(i => i.index == newIndex);
        if (!itemToReplace) {
            return;
        }
        currentItem.index = newIndex;
        itemToReplace.index = curIndex;
        this.currentIndex = newIndex;
        items.sort(this.sortColumns);
        this.setState({ items }, this.callProps.bind(this));
    }

    moveItem(items, id, index) {
        let item = items.find(i => i.id == id);
        item.index = index;
    }

    updateIndexes(items, index) {
        items.forEach((item) => {
            if (item.index >= index) {
                item.index = item.index + 1;
            }
        });
        return items;
    }

    sortOnDrag(event, dropX, dropY) {
        let id = event.draggable._element.getAttribute("data-dnn-sortable-id");
        const itemElement = ReactDOM.findDOMNode(this.refs.dnnSortable).querySelectorAll(`[data-dnn-sortable-id="${id}"]`)[0];
        itemElement.getAttribute("data-index");
        const sortableItems = document.getElementsByClassName("sortable-item");
        let newIndex = this.getNewIndex(sortableItems, dropY);

        if (this.currentIndex == -1) {
            const currentIndex = itemElement.getAttribute("data-index");
            this.currentIndex = currentIndex;
        }

        const isSameIndex = newIndex == this.currentIndex;
        if (isSameIndex) {
            return;
        }
        this.sortItem(newIndex);
    }

    onDrop(event, dropX, dropY) {
        this.currentIndex = -1;
        this.unselectAll();
        this.setState({ isDraggingOver: false });
        if (this.props.sortOnDrag) {
            return;
        }
        let id = event.draggable._element.getAttribute("data-dnn-sortable-id");
        const itemElement = ReactDOM.findDOMNode(this.refs.dnnSortable).querySelectorAll(`[data-dnn-sortable-id="${id}"]`)[0];
        itemElement.classList.remove("sortable-selected");
        this.removePlaceholder();
        const sortableItems = document.getElementsByClassName("sortable-item");
        let newIndex = this.getNewIndex(sortableItems, dropY);
        const currentIndex = event.draggable._element.getAttribute("data-index");
        const isSameIndex = currentIndex != null && (newIndex == currentIndex || newIndex - 1 == currentIndex);
        if (isSameIndex) {
            return;
        }

        let {items} = this.state;
        items = this.updateIndexes(items, newIndex);
        this.moveItem(items, id, newIndex);
        this.resetIndexes(items);
        this.setState({ items }, this.callProps.bind(this));
    }

    callProps() {
        const items = this.state.items.map(item => item.item);
        this.props.onSort(items);
    }



    resetIndexes(items = this.state.items) {
        items.sort(this.sortColumns);
        items.forEach((item, index) => {
            item.index = index;
        });
    }


    render() {
        const {children, items} = this.props;
        if (children.length != items.length) {
            console.error("Children.length and items.length should be equal");
            return false;
        }
        const listItems = children.map((child, index) => {
            const item = this.state.items[index] || {};
            return React.cloneElement(child, {
                listItem: item,
                selected: item.selected
            });
        });

        return <div className="dnn-sortable" ref="dnnSortable">
            <SortableContainer
                onDragMove={this.onDragMove.bind(this) }
                onDragStart={this.onDragStart.bind(this) }
                onDragEnd={this.onDragEnd.bind(this) }
                onDrop={this.onDrop.bind(this) }
                onDropMove={this.onDropMove.bind(this) }
                onDragLeave={this.onDragLeave.bind(this) }
                isDragging={this.state.dragging.isDragging}
                isDraggingOver={this.state.isDraggingOver}
                >
                {listItems}
            </SortableContainer>
        </div>;
    }
}

Sortable.propTypes = {
    children: PropTypes.node.isRequired,
    items: PropTypes.array.isRequired,
    onSort: PropTypes.func.isRequired,

    sortOnDrag: PropTypes.bool
};