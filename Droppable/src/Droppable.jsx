import React from "react";
import ReactDOM from "react-dom";
import interact from "interact.js";

export default (Component) => { 
    
    class Droppable extends React.Component {
    
        raiseEvent(event, callback) {
            const dropZoneRect = event.dropzone._element.getBoundingClientRect();
            const elementRect = event.relatedTarget.getBoundingClientRect();            
            const x = elementRect.left - dropZoneRect.left + elementRect.width / 2;
            const y = elementRect.top - dropZoneRect.top + elementRect.height / 2;
            if (callback) {
                callback(event, x, y);
            }
        }
        
        onDrop(event) {
            this.raiseEvent(event, this.props.onDrop);
        }
    
        onDropMove(event) {
            this.raiseEvent(event, this.props.onDropMove);
        }
    
        componentDidMount() {            
            const node = ReactDOM.findDOMNode(this);
            if (!node) {
                return;
            }
            interact.dynamicDrop(true);
            
            interact(ReactDOM.findDOMNode(this)).dropzone({
                accept: ".drag-element",
                overlap: 0.50,
                ondropactivate: this.props.onDropActivate,
                ondragenter: this.props.onDragEnter,
                ondragleave:  this.props.onDragLeave,
                ondrop: this.onDrop.bind(this),
                ondropmove: this.onDropMove.bind(this),
                ondropdeactivate: this.props.onDropDeactivate
            });
        }
        
        render() {
            return (<Component {...this.props} />);
        }
    }
    Droppable.propTypes = {     
        onDrop: React.PropTypes.func,
        onDropMove: React.PropTypes.func,
        onDropActivate: React.PropTypes.func,
        onDragEnter: React.PropTypes.func,
        onDragLeave: React.PropTypes.func,
        onDropDeactivate: React.PropTypes.func
    };
    return Droppable;
};