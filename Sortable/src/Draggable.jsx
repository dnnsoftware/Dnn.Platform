import React from "react";
import ReactDOM from "react-dom";
import interact from "interact.js";

//TODO: reconsider location of this general component
export default (Component) => { 
    
    class Draggable extends React.Component {

        showGhostTargetOnClone(currentTarget) {            
            this.originalTarget = currentTarget;
            this.originalTargetClassName = this.originalTarget.className;
            this.originalTarget.className += " ghost";            
        }
        
        setCloneInitialPositionAndWidth(clone, currentTarget) {            
            const rect = currentTarget.getBoundingClientRect();  
            Object.assign(clone.style, {
                left: rect.left + "px",
                top: rect.top + "px",
                width: rect.width + "px"
            });  
        }
        
        setPreviewInitialPosition(preview, event) {
            Object.assign(preview.style, {
                display: "",
                visibility: ""
            });       
            const rect = preview.getBoundingClientRect(); 
            Object.assign(preview.style, {
                left: (event.clientX - rect.width / 2 ) + "px",
                top: (event.clientY - rect.height / 2)  + "px"
            }); 
        }
        
        cloneTarget(currentTarget) {
            let clone = currentTarget.cloneNode(true);                
            this.setDraggingStyle(clone);
            this.setCloneInitialPositionAndWidth(clone, currentTarget);
            document.body.appendChild(clone);
            return clone;
        }
        
        clonePreviewElement(previewElement, event) {            
            let clone = previewElement.cloneNode(true);                 
            this.setDraggingStyle(clone);
            document.body.appendChild(clone);
            this.setPreviewInitialPosition(clone, event);
            return clone;
        }

        destroyClone(clone) {
            if(clone.parentNode) {
                clone.parentNode.removeChild(clone);
            }
        }
        
        setDraggingStyle(target) {
            this.previousPosition = target.style.position;
            Object.assign(target.style, {            
                position: "fixed",
                zIndex: 100000,
                boxShadow: "20px 20px 20px 2px rgba(0,0,0,0.2)"
            });
        }
        
        resetTarget(target) {   
            Object.assign(target.style, {
                position: this.previousPosition,
                zIndex: "",
                transform: "",
                webkitTransform: "",
                mozTransform: "",
                msTransform: "",
                boxShadow: ""
            });
            target.setAttribute("data-x", "");
            target.setAttribute("data-y", "");
        }
        
        moveTarget(event) {
            const target = event.target;
            const x = (parseFloat(target.getAttribute("data-x")) || 0) + event.dx;
            const y = (parseFloat(target.getAttribute("data-y")) || 0) + event.dy;
            // translate the element
            target.style.webkitTransform = 
                target.style.transform = 
                target.style.mozTransform = 
                target.style.msTransform = "translate(" + x + "px, " + y + "px)";
            
            // update the position attributes
            target.setAttribute("data-x", x);
            target.setAttribute("data-y", y);
        }

        componentDidMount() {
            const self = this;
            const dragElement = ReactDOM.findDOMNode(this);   
            if (!dragElement) {
                return;
            }     
            const {
                 cloneElementOnDrag,
                 onDragStart, 
                 onDragMove,
                 onDragEnd,
                 getDragPreview,
                 showGhostOnClone
            } = this.props;
            
            interact(dragElement).ignoreFrom("input, *[contenteditable=true], .ignoreDraggable").draggable({
                manualStart: cloneElementOnDrag,
                onmove: (event) => {                            
                    this.moveTarget(event);
                    
                    // Notify drag move
                    if(typeof(onDragMove) === "function") {
                        onDragMove(self, event);
                    }
                },
                onend: (event) => { 
                    if (cloneElementOnDrag) {
                        this.destroyClone(event.target);    
                        if (this.props.showGhostOnClone) {  
                            this.originalTarget.className = this.originalTargetClassName;
                        }
                    } else {
                        this.resetTarget(event.target);
                    }
                    
                    if (typeof(onDragEnd) === "function") {
                        onDragEnd(self, event);
                    }
                }
            }).on("move", (event) => {
                
                // if the pointer was moved while being held down
                // and an interaction has not started yet
                const interaction = event.interaction;
                if (interaction.pointerIsDown && !interaction.interacting() && interaction.prepared.name === "drag") {  
                    const currentTarget = event.currentTarget;   
                    
                    if(!cloneElementOnDrag) {            
                        this.setDraggingStyle(currentTarget);
                        return;
                    }
                    
                    let clone = null;
                    if(typeof(getDragPreview) === "function") {
                        const dragPreviewComponent = getDragPreview(self);
                        const dragPreviewDOMElement = ReactDOM.findDOMNode(dragPreviewComponent);
                        clone = this.clonePreviewElement(dragPreviewDOMElement, event);
                    } else {                                    
                        clone = this.cloneTarget(currentTarget);  
                    }
                    if (showGhostOnClone) { 
                        this.showGhostTargetOnClone(currentTarget);     
                    } 
                    // start a drag interaction targeting the clone
                    interaction.start({ name: "drag" }, event.interactable, clone);
                }
            }).on("dragstart", (event) => {                
                if (typeof(onDragStart) === "function") {
                    onDragStart(self, event);
                }
            });
        }

        render() {
            return <Component {...this.props} />;
        }
    }
    
    Draggable.propTypes = {
        onDragStart: React.PropTypes.func,
        onDragMove: React.PropTypes.func,
        onDragEnd: React.PropTypes.func,
        cloneElementOnDrag: React.PropTypes.bool.isRequired,
        getDragPreview: React.PropTypes.func,
        showGhostOnClone: React.PropTypes.bool.isRequired
    };
    
    Draggable.defaultProps = {
        showGhostOnClone: false
    };
    
    return Draggable;
};